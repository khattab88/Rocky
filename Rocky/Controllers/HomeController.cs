using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocky.Data;
using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IApplicationTypeRepository _applicationTypeRepo;
        private readonly IProductRepository _productRepo;

        public HomeController(
            ILogger<HomeController> logger,
            ICategoryRepository categoryRepo,
            IApplicationTypeRepository applicationTypeRepo,
            IProductRepository productRepo)
        {
            _logger = logger;
            _categoryRepo = categoryRepo;
            _applicationTypeRepo = applicationTypeRepo;
            _productRepo = productRepo;
        }

        public IActionResult Index()
        {
            var vm = new HomeVM 
            {
                Products = _productRepo.GetAll(includedProperties: "Category,ApplicationType"),
                Categories = _categoryRepo.GetAll()
            };

            return View(vm);
        }

        public IActionResult Details(int? id) 
        {
            if(id == null || id == 0) return NotFound();

            var product = _productRepo.FirstOrDefault(x => x.Id == id, includedProperties: "Category,ApplicationType");

            if(product == null) return NotFound();


            var vm = new DetailsVM { Product = product };

            // ckeck if product already added to cart
            var cartItems = GetCartItemsFromSession();
            if(cartItems.Count > 0)
            {
                foreach (var item in cartItems)
                {
                    if(item.ProductId == id) 
                    {
                        vm.InCart = true;
                    }
                }
            }

            return View(vm);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int? id, DetailsVM detailsVM)
        {
            if (id == null || id == 0) return NotFound();

            var cartItems = GetCartItemsFromSession();

            // add product to cart items
            cartItems.Add(new ShoppingCartItem 
            {
                ProductId = id.Value,
                SqFt = detailsVM.Product.TempSqft
            });
            HttpContext.Session.Set<List<ShoppingCartItem>>(Constants.SessionCart, cartItems);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var cartItems = GetCartItemsFromSession();

            // remove product from cart items
            var itemToRemove = cartItems.SingleOrDefault(p => p.ProductId == id);
            if (itemToRemove != null) 
            {
                var index = cartItems.IndexOf(itemToRemove);
                cartItems.RemoveAt(index);
            }

            HttpContext.Session.Set<List<ShoppingCartItem>>(Constants.SessionCart, cartItems);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private List<ShoppingCartItem> GetCartItemsFromSession() 
        {
            List<ShoppingCartItem> cartSession = null;
            List<ShoppingCartItem> cartItems = new List<ShoppingCartItem>();

            if (HttpContext.Session.Get(Constants.SessionCart) != null)
            {
                cartSession = HttpContext.Session.Get<List<ShoppingCartItem>>(Constants.SessionCart);
            }

            if (cartSession != null && cartSession.Count > 0)
            {
                cartItems = cartSession;
            }

            return cartItems;
        }
    }
}
