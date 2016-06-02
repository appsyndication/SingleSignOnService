using System.Threading.Tasks;
using AppSyndication.SingleSignOnService.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.SingleSignOnService.Web.Controllers
{
    public class Logout : Controller
    {
        private readonly ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public Logout(SignInManager<ApplicationUser> signInManager, ILoggerFactory loggerFactory)
        {
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<Logout>();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string id)
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation(4, "User logged out.");
            return View();
        }
    }
}
