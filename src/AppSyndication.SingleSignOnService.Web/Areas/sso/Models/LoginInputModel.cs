using System.ComponentModel.DataAnnotations;

namespace AppSyndication.SingleSignOnService.Web.Models
{
    public class LoginInputModel
    {
        [Required]
        [Display(Name = "Username", Prompt ="Username or email")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Prompt = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string SignInId { get; set; }
    }
}
