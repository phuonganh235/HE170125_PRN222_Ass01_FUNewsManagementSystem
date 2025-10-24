using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class AccountEditViewModel
    {
        [Required]
        public short AccountId { get; set; }

        [Required(ErrorMessage = "Account name is required")]
        [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
        [Display(Name = "Email")]
        public string AccountEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Range(1, 3, ErrorMessage = "Please select a valid role")]
        [Display(Name = "Role")]
        public int AccountRole { get; set; }

        public List<RoleOption> RoleOptions { get; set; } = new List<RoleOption>();
    }
}
