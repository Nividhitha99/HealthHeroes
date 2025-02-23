using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Health_Heroes.Models
{
    public class Donor_Details
    {
        [Key]
        [Required(ErrorMessage = "SSN is required")]
        [StringLength(11, ErrorMessage = "SSN must be in the format xxx-xx-xxxx")]
        [RegularExpression(@"\d{3}-\d{2}-\d{4}", ErrorMessage = "SSN must be in the format xxx-xx-xxxx")]
        public string SSN { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(255)]
        [ForeignKey("User")]  // Foreign key linking to the User table
        public string Email_Address { get; set; }

        // Navigation property to the User table
        public virtual  User ? User { get; set; }

        // Nullable fields
        public int? Age { get; set; }

        [StringLength(3)]
        [RegularExpression(@"^[ABO]{1,2}[+-]$", ErrorMessage = "Please enter a valid Blood Type such as A+, B-, etc.")]
        public string? Blood_Type { get; set; }

        [StringLength(15)]
        [RegularExpression(@"\d{3}-\d{3}-\d{4}", ErrorMessage = "Phone number must be in the format xxx-xxx-xxxx")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(255)]
        public string? Illness { get; set; }

        public double? Height { get; set; }

        public double? Weight { get; set; }

        public char? Gender { get; set; }
    }
}
