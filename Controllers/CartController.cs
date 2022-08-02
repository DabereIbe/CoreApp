using CoreApp.Data;
using CoreApp.Models;
using CoreApp.Models.ViewModels;
using CoreApp.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PayStack.Net;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly PayStackPayment _payment;
        private readonly InquiryGenerator _inquiry;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }
        public CartController(ApplicationDbContext db, 
            IWebHostEnvironment webHostEnvironment, 
            IEmailSender emailSender, 
            PayStackPayment payment,
            InquiryGenerator inquiry)
        {
            _db = db;
            _payment = payment;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _inquiry = inquiry;

        }
        public IActionResult Index()
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _db.Product.Where(u => prodInCart.Contains(u.Id));

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {

            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //var userId = User.FindFirstValue(ClaimTypes.Name);

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _db.Product.Where(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                AppUser = _db.AppUser.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = prodList.ToList()
            };


            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
        {
            int Amount = ProductUserVM.Amount;
            string Email = ProductUserVM.AppUser.Email;
            string Name = ProductUserVM.AppUser.FirstName + " " + ProductUserVM.AppUser.LastName;
            string PhoneNumber = ProductUserVM.AppUser.PhoneNumber;
            string Address = ProductUserVM.AppUser.Address;

            var subject = "New Inquiry";

            //Initializes Payment with Paystack
            if (ProductUserVM.PayOnline == true)
            {
                await _payment.InitializePayment(Amount, Email, Name, Address, _inquiry.FileName);
                if (_payment.AuthorizationUrl != null)
                {
                    return Redirect(_payment.AuthorizationUrl);
                }
                else
                {
                    ViewData["Error"] = _payment.Message;
                    return View();
                }
            }
            else
            {
                var orderModel = new Orders()
                {
                    Name = Name,
                    Amount = Amount,
                    Email = Email,
                    Address = ProductUserVM.AppUser.Address,
                };
                await _db.Orders.AddAsync(orderModel);
                await _db.SaveChangesAsync();
            }

            StringBuilder productList = new StringBuilder();
            foreach (var prod in ProductUserVM.ProductList)
            {
                productList.Append($" - Name: { prod.Name} <span style='font-size:14px;'> (${prod.Price}) </span><br />");
            }

            _inquiry.GenerateAdminInquiry(Name, Email, PhoneNumber, Address, productList.ToString());

            _inquiry.GenerateCustomerInquiry(Name, Email, PhoneNumber, Address, productList.ToString());

            //Saves Inquiry in HTML file
            string path = _webHostEnvironment.WebRootPath + @"\Inquiries\" + _inquiry.FileName;
            System.IO.File.WriteAllText(path, _inquiry.MessageBody);
            

            //Initialize HTML to PDF converter 
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);

            WebKitConverterSettings settings = new WebKitConverterSettings();

            //Set WebKit path
            settings.WebKitPath = Path.Combine(_webHostEnvironment.ContentRootPath, "QtBinariesWindows");

            //Assign WebKit settings to HTML converter
            htmlConverter.ConverterSettings = settings;

            //Convert URL to PDF
            PdfDocument document = htmlConverter.Convert(path);


            //Saving the PDF to the FileStream
            using (var fileStream = new FileStream(Path.Combine(path), FileMode.Create))
            {
                document.Save(fileStream);
            }


            //Sends Email To Admin
            await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, _inquiry.MessageBody);

            //Sends Email To Buyer
            await _emailSender.SendEmailAsync(Email, subject, _inquiry.CustomerMessageBody);

            
            
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }

        public async Task<IActionResult> Verify(string reference)
        {
            await _payment.VerifyPayment(reference);
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult Remove(int id)
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
