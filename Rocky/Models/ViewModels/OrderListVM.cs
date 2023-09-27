using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky.Utility.Enums;
using System.Collections.Generic;

namespace Rocky.Models.ViewModels
{
    public class OrderListVM
    {
        public IEnumerable<Order> OrderList { get; set; }
        public IEnumerable<SelectListItem> OrderStatusList { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
