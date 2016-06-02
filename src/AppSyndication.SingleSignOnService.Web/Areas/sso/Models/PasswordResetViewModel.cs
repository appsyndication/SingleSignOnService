using System.ComponentModel.DataAnnotations;

namespace AppSyndication.SingleSignOnService.Web.Models
{
    public class PasswordResetViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Prompt="")]
        public string Email { get; set; }
    }
}
