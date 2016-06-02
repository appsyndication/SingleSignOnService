using Microsoft.AspNetCore.Mvc;

namespace AppSyndication.SingleSignOnService.Web.Controllers
{
    public class X_UI : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string id)
        {
            return View();
        }

        public IActionResult Logout(string id)
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult ResetPassword(string id)
        {
            ViewData["Message"] = "Need a new password, eh?";

            return View();
        }

        public IActionResult ResetSent(string id)
        {
            ViewData["Message"] = "Check your email. Your password reset request should be there soon.";

            return View();
        }

        public IActionResult Error(string id)
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Consent(string id)
        {
            return View();
        }
    }
}
