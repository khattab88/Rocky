using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Models
{
    public class InquiryDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Inquiry")]
        public int InquiryId { get; set; }
        [ForeignKey("InquiryId")]
        public virtual Inquiry Inquiry { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
