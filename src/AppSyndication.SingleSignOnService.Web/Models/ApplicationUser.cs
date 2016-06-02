using FireGiant.Identity.AzureTableStorage;

namespace AppSyndication.SingleSignOnService.Web.Models
{
    public class ApplicationUser : AzureUser
    {
        public static ApplicationUser NewUser(string username, string email)
        {
            return AzureUser.NewUser<ApplicationUser>(username, email);
        }
    }
}
