using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.IO;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Rocky.Utility;
using Rocky.Data.Repositories.Interfaces;

namespace Rocky.Controllers
{
    [Authorize(Roles = Constants.Roles.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IApplicationTypeRepository _applicationTypeRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductRepository productRepo,
            ICategoryRepository categoryRepo,
            IApplicationTypeRepository applicationTypeRepo,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _applicationTypeRepo = applicationTypeRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var products = _productRepo.GetAll(includedProperties: "Category,ApplicationType");

            return View(products);
        }

        // GET - Upsert (both update & insert)
        public IActionResult Upsert(int? id) 
        {
            var productVm = new ProductVM 
            {
                Product = new Product(),
                CategorySelectList = _categoryRepo.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                ApplicationTypeSelectList = _applicationTypeRepo.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            if (id == null)
            {
                return View(productVm);
            }
            else 
            {
                productVm.Product = _productRepo.Find(id.GetValueOrDefault());

                if(productVm.Product == null) 
                {
                    return NotFound();
                }

                return View(productVm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVm) 
        {
            if (ModelState.IsValid) 
            {
                var files = HttpContext.Request.Form.Files;
                var webRootPath = _webHostEnvironment.WebRootPath;

                if (productVm.Product.Id == 0)
                {
                    // CREATE

                    // 1. upload product image
                    var uploadFolder = webRootPath + Constants.ProductImagesPath;
                    var fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    // 2. update producVM image field 
                    productVm.Product.Image = fileName + extension;

                    // 3. add product to db
                    _productRepo.Add(productVm.Product);
                }
                else 
                {
                    // UPDATE
                    var existingProduct = _productRepo.FirstOrDefault(p => p.Id == productVm.Product.Id, enableTracking: false);

                    // check if new product image has been uploaded
                    // if so, update productVM image field
                    if (files.Count > 0)
                    {
                        var uploadFolder = webRootPath + Constants.ProductImagesPath;
                        var fileName = Guid.NewGuid().ToString();
                        var extension = Path.GetExtension(files[0].FileName);

                        // remove old image
                        var oldImage = Path.Combine(uploadFolder, existingProduct.Image);
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }

                        // upload new image
                        using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productVm.Product.Image = fileName + extension;
                    }
                    else 
                    {
                        // keep old product image
                        productVm.Product.Image = existingProduct.Image;
                    }

                    _productRepo.Update(productVm.Product);
                }

                _productRepo.SaveChanges();
                return RedirectToAction("Index");
            }

            productVm.CategorySelectList = _categoryRepo.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });

            productVm.ApplicationTypeSelectList = _applicationTypeRepo.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });

            return View(productVm);
        }

        // GET - Delete
        public IActionResult Delete(int? id) 
        {
            if(id == null || id == 0) 
            {
                return NotFound();
            }

            var product = _productRepo.FirstOrDefault(p => p.Id == id, includedProperties: "Category,ApplicationType");

            if (product == null) 
            {
                return NotFound();
            }

            return View(product);
        }

        // POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id) 
        {
            if( id == null || id == 0) { return NotFound(); }

            var product = _productRepo.Find(id.GetValueOrDefault());

            if(product == null) { return NotFound(); }

            // delete product image
            var webRootPath = _webHostEnvironment.WebRootPath;
            var uploadFolder = webRootPath + Constants.ProductImagesPath;
            var oldImage = Path.Combine(uploadFolder, product.Image);
            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }

            // delete product
            _productRepo.Remove(product);
            _productRepo.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
