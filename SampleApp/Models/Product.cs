using System;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models
{
    public record Product
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
        
        public bool IsOrderable { get; set; }

        [Required]
        [Range(typeof(DateTime), "1/1/2011", "1/1/2012", ErrorMessage = "Date is out of Range")]
        public DateTime IntroductionDate { get; set; } = DateTime.Today;
    }

    public enum Status
    {
        None,
        Normal,
        Special,

        [Display(Name = "Depricated !!!")]
        Depricated = 99
    }
}