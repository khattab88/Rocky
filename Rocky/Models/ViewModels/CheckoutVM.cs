using System.Collections.Generic;

namespace Rocky.Models.ViewModels
{
    public class CheckoutVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public List<Product> ProductList { get; set; }

        public CheckoutVM()
        {
            ProductList = new List<Product>();
        }
    }
}
