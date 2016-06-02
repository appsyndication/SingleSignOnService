namespace AppSyndication.SingleSignOnService.Web.Models
{
    public class LoginViewModel : LoginInputModel
    {
        public LoginViewModel()
        {
        }

        public LoginViewModel(LoginInputModel other)
        {
            this.UserName = other.UserName;
            Password = other.Password;
            RememberMe = other.RememberMe;
            SignInId = other.SignInId;
        }

        public string ErrorMessage { get; set; }
    }
}
