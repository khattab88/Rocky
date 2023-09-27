using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using Rocky.Utility.Enums;
using Rocky.Utility.Payment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IProductRepository _productRepo;
        private readonly IInquiryRepository _inquiryRepo;
        private readonly IInquiryDetailRepository  _inquiryDetailRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderDetailRepository _orderDetailRepo;

        [BindProperty]
        public CheckoutVM CheckoutVM { get; set; }

        public CartController(
            IWebHostEnvironment webHostEnvironment,
            IEmailSender emailSender,
            IPaymentGateway paymentGateway,
            IApplicationUserRepository userRepo,
            IProductRepository productRepo,
            IInquiryRepository inquiryRepo,
            IInquiryDetailRepository inquiryDetailRepo,
            IOrderRepository orderRepo,
            IOrderDetailRepository orderDetailRepo
            )
        {
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _paymentGateway = paymentGateway;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _inquiryRepo = inquiryRepo;
            _inquiryDetailRepo = inquiryDetailRepo;
            _orderRepo = orderRepo;
            _orderDetailRepo = orderDetailRepo;
        }

        public IActionResult Index()
        {
            var cartItems = GetCartItemsFromSession();

            // select unique/distinct products from cart items
            var productIds = cartItems.Select(x => x.ProductId).ToList();
            var productListTemp = _productRepo.GetAll(p => productIds.Contains(p.Id));
            var productList = new List<Product>();

            foreach (var item in cartItems)
            {
                Product prodTemp = productListTemp.FirstOrDefault(p => p.Id == item.ProductId);
                prodTemp.TempSqft = item.SqFt;
                productList.Add(prodTemp);
            }

            return View(productList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> productList)
        {
            var shoppingCartItems = new List<ShoppingCartItem>();

            foreach (var product in productList)
            {
                shoppingCartItems.Add(new ShoppingCartItem { ProductId = product.Id, SqFt = product.TempSqft });
            }

            HttpContext.Session.Set(Constants.SessionCart, shoppingCartItems);

            return RedirectToAction(nameof(Checkout));
        }

        public IActionResult Checkout()
        {
            ApplicationUser applicationUser = null;

            if (User.IsInRole(Constants.Roles.AdminRole))
            {
                int inquiryId = 0;
                try
                {
                    inquiryId = HttpContext.Session.Get<int>(Constants.SessionInquiryId);
                }
                catch { }
                
                if (inquiryId != 0)
                {
                    // cart has been created from inquiry
                    var inaquiry = _inquiryRepo.Find(inquiryId);

                    //applicationUser = new ApplicationUser
                    //{
                    //    FullName = CheckoutVM.ApplicationUser.FullName,
                    //    Email = CheckoutVM.ApplicationUser.Email,
                    //    PhoneNumber = CheckoutVM.ApplicationUser.PhoneNumber,
                    //};

                    // get (customer) user's id
                    var userClaims = (ClaimsIdentity)User.Identity;
                    var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier).Value;
                    // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    applicationUser = _userRepo.FirstOrDefault(u => u.Id == userId);
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }

                // Payment
                var gateway = _paymentGateway.CreateGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else 
            {
                // get (customer) user's id
                var userClaims = (ClaimsIdentity)User.Identity;
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier).Value;
                // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                applicationUser = _userRepo.FirstOrDefault(u => u.Id == userId);
            }

            // get cart items
            var cartItems = GetCartItemsFromSession();
            // select unique/distinct products from cart items
            var productIds = cartItems.Select(x => x.ProductId).ToList();
            var productList = new List<Product>();

            foreach (var item in cartItems)
            {
                Product prodTemp = _productRepo.FirstOrDefault(p => p.Id == item.ProductId);
                prodTemp.TempSqft = item.SqFt;
                productList.Add(prodTemp);
            }

            var vm = new CheckoutVM
            {
                ApplicationUser = applicationUser,
                ProductList = productList
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Checkout))]
        public async Task<IActionResult> CheckoutPost(IFormCollection form, CheckoutVM vm)
        {
            // TODO: save inquiry to db
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;

            if (User.IsInRole(Constants.Roles.AdminRole))
            {
                // create an order
                var order = new Order 
                {
                    CreatedByUserId = userId,
                    OrderTotal = vm.ProductList.Sum(x => x.Price * x.TempSqft),
                    OrderDate = DateTime.Now,
                    OrderStatus = OrderStatus.Pending.ToString(),
                    FullName = vm.ApplicationUser.FullName,
                    Email = vm.ApplicationUser.Email,
                    PhoneNumber = vm.ApplicationUser.PhoneNumber,
                    Address = vm.ApplicationUser.Address,
                    City = vm.ApplicationUser.City,
                    State = vm.ApplicationUser.State,
                    PostalCode = vm.ApplicationUser.PostalCode,
                };

                _orderRepo.Add(order);
                _orderRepo.SaveChanges();

                foreach (var prod in vm.ProductList)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = prod.Id,
                        SqFt = prod.TempSqft,
                        PricePerSqFt = prod.Price
                    };

                    _orderDetailRepo.Add(orderDetail);
                }
                _orderDetailRepo.SaveChanges();

                // Payment
                string nonceFromClient = form["payment_method_nonce"];

                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(order.OrderTotal),
                    PaymentMethodNonce = nonceFromClient,
                    OrderId = order.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                var gateway = _paymentGateway.GetGateway();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if(result.Target.ProcessorResponseText == "Approved") 
                {
                    order.TransactionId = result.Target.Id;
                    order.OrderStatus = OrderStatus.Approved.ToString();
                }
                else
                {
                    order.OrderStatus = OrderStatus.Cancelled.ToString();
                }
                _orderRepo.SaveChanges();

                return RedirectToAction(nameof(InquiryConfirmation), new { id = order.Id });
            }
            else 
            {
                // create an inquiry
                var inquiry = new Inquiry
                {
                    ApplicationUserId = userId,
                    FullName = vm.ApplicationUser.FullName,
                    Email = vm.ApplicationUser.Email,
                    PhoneNumber = vm.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now
                };
                _inquiryRepo.Add(inquiry);
                _inquiryRepo.SaveChanges();

                foreach (var prod in vm.ProductList)
                {
                    var inquiryDetail = new InquiryDetail
                    {
                        InquiryId = inquiry.Id,
                        ProductId = prod.Id
                    };

                    _inquiryDetailRepo.Add(inquiryDetail);
                }
                _inquiryRepo.SaveChanges();

                var mailBody = GenerateEmail(vm);
                await _emailSender.SendEmailAsync(Constants.AdminEmail, "Rocky - Inquiry Email", mailBody);
            }

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(int id=0) 
        {
            Order order = _orderRepo.FirstOrDefault(x => x.Id == id);

            HttpContext.Session.Clear();

            return View(order);
        }

        public IActionResult Remove(int id) 
        {
            var cartItems = GetCartItemsFromSession();

            cartItems.Remove(cartItems.FirstOrDefault(i => i.ProductId == id));

            HttpContext.Session.Set<List<ShoppingCartItem>>(Constants.SessionCart, cartItems);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> productList) 
        {
            var shoppingCartItems = new List<ShoppingCartItem>();

            foreach (var product in productList) 
            {
                shoppingCartItems.Add(new ShoppingCartItem { ProductId = product.Id, SqFt = product.TempSqft });
            }

            HttpContext.Session.Set(Constants.SessionCart, shoppingCartItems);

            return RedirectToAction(nameof(Index)); 
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        private List<ShoppingCartItem> GetCartItemsFromSession() 
        {
            var cartItems = new List<ShoppingCartItem>();

            // get products from cart session
            if (HttpContext.Session.Get(Constants.SessionCart) != null)
            {
                var itemsFromCartSession = HttpContext.Session.Get<List<ShoppingCartItem>>(Constants.SessionCart);
                if (itemsFromCartSession != null && itemsFromCartSession.Count > 0)
                {
                    cartItems = itemsFromCartSession;
                }
            }

            return cartItems;
        }

        private string GenerateEmail(CheckoutVM vm) 
        {
            var pathToEmailTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "templates" + Path.DirectorySeparatorChar.ToString() +
                "inquiry.html";
            
            var htmlBody = string.Empty;

            using (var reader = System.IO.File.OpenText(pathToEmailTemplate))
            {
                htmlBody = reader.ReadToEnd();
            }

            /* 
             * Name: {0}
             * Email: {1}
             * Phone: {2}
             * Product List: {3}
            */
            var productListSB = new StringBuilder();
            foreach (var prod in vm.ProductList)
            {
                productListSB.Append($" - Name: {prod.Name} <span style='font-size=14px;'>(ID: {prod.Id})</span>");
            }

            var body = string.Format(htmlBody,
                vm.ApplicationUser.FullName,
                vm.ApplicationUser.Email,
                vm.ApplicationUser.PhoneNumber,
                productListSB.ToString());

            return body;
        }
    }
}
