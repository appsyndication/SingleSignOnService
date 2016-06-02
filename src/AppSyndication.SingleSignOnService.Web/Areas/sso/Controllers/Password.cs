using System;
using System.Threading.Tasks;
using AppSyndication.SingleSignOnService.Web.Models;
using FireGiant.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.SingleSignOnService.Web.Controllers
{
    public class Password : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;

        public Password(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<Login>();
        }

        public async Task<IActionResult> Reset()
        {
            // If the user is already signed in, send a password reset request to the
            // email address we have associated with their account.
            if (_signInManager.IsSignedIn(this.User))
            {
                var user = await _userManager.GetUserAsync(this.User);

                // Send an email with this link
                await this.SendPasswordResetEmail(user);

                return this.View(nameof(Password.Sent));
            }

            // Otherwise, prompt the user for their username or email so we can send
            // the password reset request to them.
            //
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(PasswordResetViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userManager.FindByNameOrEmailAsync(model.Email);

                if (user != null && (await _userManager.IsEmailConfirmedAsync(user)))
                {
                    await this.SendPasswordResetEmail(user);
                }
                // else don't reveal that the user does not exist or is not confirmed.

                return this.RedirectToAction(nameof(Password.Sent));
            }

            return this.View(model);
        }

        public ActionResult Change(string code)
        {
            return String.IsNullOrEmpty(code) ? this.View("Error") : this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Change(PasswordChangeViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userManager.FindByNameOrEmailAsync(model.UserName);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return this.RedirectToAction(nameof(Password.Changed));
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return this.RedirectToAction(nameof(Password.Changed));
                }

                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return this.View(model);
        }

        public IActionResult Sent()
        {
            return this.View();
        }

        public IActionResult Changed()
        {
            return this.View();
        }

        private async Task SendPasswordResetEmail(ApplicationUser user)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = this.Url.Action(nameof(Password.Change), nameof(Password), new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            //await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
        }
    }
}
