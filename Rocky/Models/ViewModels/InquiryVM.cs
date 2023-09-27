using System.Collections.Generic;

namespace Rocky.Models.ViewModels
{
    public class InquiryVM
    {
        public Inquiry Inquiry { get; set; }
        public List<InquiryDetail> InquiryDetails { get; set; }
    }
}
