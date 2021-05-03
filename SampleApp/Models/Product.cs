using System;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models
{
    public class Product
    {

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Range(1, 10000)]
        [Display(Name = "Price")]
        public decimal? UnitPrice { get; set; }

        [Range(0, 100)]
        [Required]
        [Display(Name = "Units in stock")]
        public int UnitsInStock { get; set; }

        public Status Status { get; set; }
    }

    public enum Status
    {
        None,
        Normal,
        Special,
        Depricated
    }
}