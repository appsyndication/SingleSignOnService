using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AppSyndication.SingleSignOnService.Web.Identity
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder ConfigureIdentity<TUser>(this IIdentityServerBuilder builder)
            where TUser : class
        {
            var services = builder.Services;

            services.AddTransient<SignInManager<TUser>, IdentityServerSignInManager<TUser>>();
            services.AddTransient<IProfileService, ProfileService<TUser>>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator<TUser>>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.AuthenticationScheme = Constants.PrimaryAuthenticationType;

                options.Cookies.ApplicationCookie.LoginPath = new PathString("/sso/" + Constants.RoutePaths.Login);
                options.Cookies.ApplicationCookie.LogoutPath = new PathString("/sso/" + Constants.RoutePaths.Logout);

                options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
                options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
                options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
            });

            services.Configure<IdentityServerOptions>(
                options =>
                {
                    options.AuthenticationOptions.PrimaryAuthenticationScheme = Constants.PrimaryAuthenticationType;
                });

            return builder;
        }
    }
}
