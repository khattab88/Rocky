using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Rocky.Controllers
{
    [Authorize(Roles = Constants.Roles.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryRepository _inquiryRepo;
        private readonly IInquiryDetailRepository _inquiryDetailRepo;

        [BindProperty]
        public InquiryVM InquiryVM { get; set; }

        public InquiryController(
            IInquiryRepository inquiryRepo,
            IInquiryDetailRepository inquiryDetailRepo)
        {
            _inquiryRepo = inquiryRepo;
            _inquiryDetailRepo = inquiryDetailRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id) 
        {
            var vm = new InquiryVM 
            {
                Inquiry = _inquiryRepo.Find(id),
                InquiryDetails = _inquiryDetailRepo.GetAll(i => i.InquiryId == id, includedProperties: "Product").ToList(),
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            InquiryVM.InquiryDetails = _inquiryDetailRepo.GetAll(i => i.Inquiry.Id == InquiryVM.Inquiry.Id).ToList();

            var shoppingCartList = new List<ShoppingCartItem>();
            foreach (var detail in InquiryVM.InquiryDetails)
            {
                var shoppingCartItem = new ShoppingCartItem() { ProductId = detail.ProductId };
                shoppingCartList.Add(shoppingCartItem);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.Set(Constants.SessionCart, shoppingCartList);
            HttpContext.Session.Set(Constants.SessionInquiryId, InquiryVM.Inquiry.Id);

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete() 
        {
            var inquiry = _inquiryRepo.FirstOrDefault(i => i.Id == InquiryVM.Inquiry.Id);
            var inquiryDetails = _inquiryDetailRepo.GetAll(d => d.InquiryId == InquiryVM.Inquiry.Id);

            _inquiryDetailRepo.RemoveRange(inquiryDetails);
            _inquiryRepo.Remove(inquiry);
            _inquiryRepo.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        #region API

        [HttpGet]
        public IActionResult GetInquiryList() 
        {
            return Json(new { data = _inquiryRepo.GetAll() });
        }

        #endregion
    }
}
