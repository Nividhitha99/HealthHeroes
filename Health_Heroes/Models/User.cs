using System.ComponentModel.DataAnnotations;

namespace Health_Heroes.Models
{
    public class User
    {
        [Key]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(255)]
        public string Email_Address { get; set; }  // Primary key

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255)]
        public string Password { get; set; }
    }
}

