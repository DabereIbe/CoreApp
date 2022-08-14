using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreApp.Data;
using CoreApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CoreApp.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<SelectListItem> GetCategory()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var cat = _db.Category.ToList();
            foreach (var item in cat)
            {
                list.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Name });
            }
            return list;
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        
        public IActionResult Categories()
        {
            IEnumerable<Category> data = _db.Category;
            return View(data);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category obj)
        {
            _db.Category.Add(obj);
            _db.SaveChanges();
            return RedirectToAction("Categories");
        }

        public IActionResult EditCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Category.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(Category obj)
        {
            _db.Category.Update(obj);
            _db.SaveChanges();
            return RedirectToAction("Categories");
        }

        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Category.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategoryConfirm(int? id)
        {
            var obj = _db.Category.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Category.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Categories");
        }

        public IActionResult Products()
        {
            IEnumerable<Product> objList = _db.Product;
            return View(objList);
        }

        public IActionResult CreateProduct()
        {
            ViewBag.CategoryList = GetCategory();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product obj, IFormFile file)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            if (obj.Id > 0)
            {
                string upload = webRootPath + WC.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(file.FileName).ToLower();

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.Image = fileName + extension;

            }


            _db.Product.Add(obj);
            _db.SaveChanges();
            return RedirectToAction("Products");
        }

        public IActionResult EditProduct(int? id)
        {
            ViewBag.CategoryList = GetCategory();
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Product.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Product obj, IFormFile file)
        {
            var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == obj.Id);
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (file != null)
            {
                string upload = webRootPath + WC.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(file.FileName);

                var oldFile = Path.Combine(upload, objFromDb.Image);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.Image = fileName + extension;
            }
            else
            {
                obj.Image = objFromDb.Image;
            }
            _db.Product.Update(obj);
            _db.SaveChanges();
            return RedirectToAction("Products");
        }

        public IActionResult DeleteProduct(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Product.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductConfirm(int? id)
        {
            var obj = _db.Product.Find(id);
            var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == obj.Id);
            string webRootPath = _webHostEnvironment.WebRootPath;
            string upload = webRootPath + WC.ImagePath;

            if (obj == null)
            {
                return NotFound();
            }
            var oldFile = Path.Combine(upload, objFromDb.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _db.Product.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Products");
        }

        public IActionResult Orders()
        {
            var orders = _db.Orders.ToList();
            return View(orders);
        }

        public IActionResult ViewOrder(string id)
        {
            var order = _db.Orders.Find(id);
            return View(order);
        }

        public IActionResult DeleteOrder(string id)
        {
            var order = _db.Orders.Find(id);
            
            _db.Orders.Remove(order);
            _db.SaveChanges();
            return RedirectToAction("Orders");
        }
    }
}