namespace Rocky.Models.ViewModels
{
    public class DetailsVM
    {
        public Product Product { get; set; }
        public bool InCart { get; set; }

        public DetailsVM()
        {
            Product = new Product();
            InCart = false;
        }
    }
}
