using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AppSyndication.SingleSignOnService.Web.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.SingleSignOnService.Web.Controllers
{
    public class Home : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public Home(UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<Home>();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }


        public async Task<IActionResult> Create()
        {
            var username = "robmen";

            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                return Ok(user);
            }

            user = ApplicationUser.NewUser(username,"rob@robmensching.com");

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                var claims = new[]
                {
                    new Claim(JwtClaimTypes.Role, "admin"),
                    new Claim(JwtClaimTypes.GivenName, "Rob"),
                    new Claim(JwtClaimTypes.FamilyName, "Menschiing"),
                    new Claim(JwtClaimTypes.Name, "Rob Menschiing"),
                };

                result = await _userManager.AddClaimsAsync(user, claims);
            }

            return result.Succeeded ? (IActionResult)Created("robmen", user) : (IActionResult)BadRequest(String.Join("\r\n", result.Errors.Select(e => e.Code + ": " + e.Description)));
        }
    }
}
