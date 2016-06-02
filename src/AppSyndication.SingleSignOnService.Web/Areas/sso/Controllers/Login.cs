using System;
using System.Threading.Tasks;
using AppSyndication.SingleSignOnService.Web.Models;
using FireGiant.Identity.Extensions;
using IdentityServer4.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.SingleSignOnService.Web.Controllers
{
    public class Login : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SignInInteraction _signInInteraction;
        private readonly ILogger _logger;

        public Login(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, SignInInteraction signInInteraction, ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _signInInteraction = signInInteraction;
            _logger = loggerFactory.CreateLogger<Login>();
        }

        public async Task<IActionResult> Index(string id = null, string returnUrl = null)
        {
            var model = new LoginViewModel();
            if (id != null)
            {
                var request = await _signInInteraction.GetRequestAsync(id);
                if (request != null)
                {
                    model.UserName = request.LoginHint;
                    model.SignInId = id;
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginInputModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Require the user to have a confirmed email before they can log on.
                var user = await _userManager.FindByNameOrEmailAsync(model.UserName);

                if (user != null)
                {
                    if (await _userManager.IsEmailConfirmedAsync(user))
                    {
                        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);

                        if (result.Succeeded)
                        {
                            _logger.LogInformation(1, "User logged in.");

                            return (model.SignInId == null) ? RedirectToLocal(returnUrl) : new Models.SignInResult(model.SignInId);
                        }

                        //if (result.RequiresTwoFactor)
                        //{
                        //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                        //}

                        if (result.IsLockedOut)
                        {
                            _logger.LogWarning(2, "User account locked out.");

                            return View("Lockout");
                        }
                    }
                    else // users must confirm their email address.
                    {
                        // TODO: add link to page to resend email confirmation.

                        ModelState.AddModelError(String.Empty, "You must have a confirmed email to log in.");

                        return View(new LoginViewModel(model));
                    }
                }

                ModelState.AddModelError(String.Empty, "Invalid login attempt.");
            }

            // If we got this far, something failed, redisplay form
            return View(new LoginViewModel(model));
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Home.Index), "Home");
            }
        }
    }
}
