namespace Rocky.Utility
{
    public static class Constants
    {
        public const string ProductImagesPath = @"\images\products\";
        public const string SessionCart = "ShoppingCartSession";
        public const string SessionInquiryId = "InquirySession";

        public const string AdminEmail = "admin@rocky.com";

        public static class Roles
        {
            public const string AdminRole = "Admin";
            public const string CustomerRole = "Customer";
        }

        public static class Notifications 
        {
            public const string Success = "Success";
            public const string Error = "Error";
        }
    }
}
