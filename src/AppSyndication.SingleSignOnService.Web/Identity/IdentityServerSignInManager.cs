using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSyndication.SingleSignOnService.Web.Identity
{
    public class IdentityServerSignInManager<TUser> : SignInManager<TUser>
        where TUser : class
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IdentityOptions _options;

        public IdentityServerSignInManager(
            UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger)
        {
            _contextAccessor = contextAccessor;
            _options = optionsAccessor.Value;
        }

        public override async Task SignInAsync(TUser user, AuthenticationProperties authenticationProperties, string authenticationMethod = null)
        {
            var principal = await CreateUserPrincipalAsync(user);
            var identity = principal.Identities.First();

            identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, Constants.BuiltInIdentityProvider));
            identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString()));

            if (authenticationMethod != null)
            {
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, authenticationMethod));
            }

            await _contextAccessor?.HttpContext.Authentication.SignInAsync(_options.Cookies.ApplicationCookieAuthenticationScheme, principal, authenticationProperties ?? new AuthenticationProperties());
        }
    }
}
