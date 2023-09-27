using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using Rocky.Utility.Enums;
using Rocky.Utility.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Braintree;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rocky.Controllers
{
    [Authorize(Roles = Constants.Roles.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderDetailRepository _orderDetailRepo;
        private readonly IPaymentGateway _paymentGateway;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(
            IOrderRepository orderRepo,
            IOrderDetailRepository orderDetailRepo,
            IPaymentGateway paymentGateway)
        {
            _orderRepo = orderRepo;
            _orderDetailRepo = orderDetailRepo;
            _paymentGateway = paymentGateway;
        }

        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string OrderStatus = null)
        {
            var vm = new OrderListVM
            {
                OrderList = _orderRepo.GetAll(),
                OrderStatusList = Enum.GetNames(typeof(OrderStatus)).Select(o => new SelectListItem { Text = o, Value = o })
            };

            if (!string.IsNullOrEmpty(searchName)) 
            {
                vm.OrderList = vm.OrderList.Where(o => o.FullName.ToLower().Contains(searchName.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchEmail))
            {
                vm.OrderList = vm.OrderList.Where(o => o.Email.ToLower().Contains(searchEmail.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchPhone))
            {
                vm.OrderList = vm.OrderList.Where(o => o.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }

            if (!string.IsNullOrEmpty(OrderStatus) && OrderStatus != "--Order Status--")
            {
                vm.OrderList = vm.OrderList.Where(o => o.OrderStatus.ToLower().Contains(OrderStatus.ToLower()));
            }

            return View(vm);
        }

        public IActionResult Details(int? id) 
        {
            if (id == null) return NotFound();

            OrderVM = new OrderVM() 
            {
                Order = _orderRepo.FirstOrDefault(o => o.Id == id),
                OrderDetails = _orderDetailRepo.GetAll(d => d.OrderId == id, includedProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult StartProcessing() 
        {
            var order = _orderRepo.FirstOrDefault(o => o.Id == OrderVM.Order.Id);
            order.OrderStatus = OrderStatus.InProcess.ToString();
            _orderRepo.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            var order = _orderRepo.FirstOrDefault(o => o.Id == OrderVM.Order.Id);
            order.OrderStatus = OrderStatus.Shipped.ToString();
            order.ShippingDate = DateTime.Now;
            _orderRepo.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            var order = _orderRepo.FirstOrDefault(o => o.Id == OrderVM.Order.Id);

            // payment (refund)
            var gateway = _paymentGateway.GetGateway();
            Braintree.Transaction transaction = gateway.Transaction.Find(order.TransactionId);

            Result<Braintree.Transaction> result = null;
            if (transaction.Status == Braintree.TransactionStatus.AUTHORIZED ||
               transaction.Status == Braintree.TransactionStatus.SUBMITTED_FOR_SETTLEMENT) 
            {
                // no refund
                result = gateway.Transaction.Void(order.TransactionId);
            }
            else
            {
                // refund
                result = gateway.Transaction.Refund(order.TransactionId);

            }

            order.OrderStatus = OrderStatus.Refunded.ToString();
            _orderRepo.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateOrderDetails() 
        {
            var orderFromDb = _orderRepo.FirstOrDefault(o => o.Id == OrderVM.Order.Id);

            orderFromDb.FullName = OrderVM.Order.FullName;
            orderFromDb.Email = OrderVM.Order.Email;
            orderFromDb.PhoneNumber = OrderVM.Order.PhoneNumber;
            orderFromDb.Address = OrderVM.Order.Address;
            orderFromDb.City = OrderVM.Order.City;
            orderFromDb.State = OrderVM.Order.State;
            orderFromDb.PostalCode = OrderVM.Order.PostalCode;

            _orderDetailRepo.SaveChanges();

            TempData[Constants.Notifications.Success] = "Order updated successfully";

            return RedirectToAction(nameof(Details), new { id = orderFromDb.Id });
        }
    }
}
