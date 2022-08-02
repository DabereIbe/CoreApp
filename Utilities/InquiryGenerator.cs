using CoreApp.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Utilities
{
    public class InquiryGenerator
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InquiryGenerator(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string MessageBody { get; set; }
        public string CustomerMessageBody { get; set; }
        public string FileName { get; set; }

        public void GenerateAdminInquiry(string name, string email, string phoneNumber, string address, string productList)
        {
            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() +
                "Inquiry.html";

            string HtmlBody = "";
            using (StreamReader sr = File.OpenText(PathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }
            
            //StringBuilder productListSB = new StringBuilder();
            //foreach (var prod in ProductUserVM.ProductList)
            //{
            //    productListSB.Append($" - Name: { prod.Name} <span style='font-size:14px;'>  </span><br />");
            //}
            string filename = DateTime.Now.Ticks.ToString() + "_" + "New Inquiry.html";
            string messageBody = string.Format(HtmlBody,
                name,
                email,
                phoneNumber,
                DateTime.Now.ToString(),
                productList,
                address,
                filename);
            FileName = filename;
            MessageBody = messageBody;
        }

        public void GenerateCustomerInquiry(string name, string email, string phoneNumber, string address, string productList)
        {
            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() +
                "CustomerInquiry.html";

            string HtmlBody = "";
            using (StreamReader sr = File.OpenText(PathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }

            
            string messageBody = string.Format(HtmlBody,
                name,
                email,
                phoneNumber,
                DateTime.Now.ToString(),
                productList,
                address,
                FileName);
            CustomerMessageBody = messageBody;
        }
    }
}
