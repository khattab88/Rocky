﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        [NotMapped]
        public string Address { get; set; }
        [NotMapped]
        public string City { get; set; }
        [NotMapped]
        public string State { get; set; }
        [NotMapped]
        public string PostalCode { get; set; }
    }
}
