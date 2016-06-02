using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using Microsoft.AspNetCore.Identity;

namespace AppSyndication.SingleSignOnService.Web.Identity
{
    public class ProfileService<TUser> : IProfileService where TUser : class
    {
        private readonly UserManager<TUser> _userManager;

        public ProfileService(UserManager<TUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());

            var claims = await GetClaims(user);

            if (!context.AllClaimsRequested)
            {
                claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Subject == null)
            {
                throw new ArgumentNullException(nameof(context.Subject));
            }

            context.IsActive = false;

            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());

            if (user != null)
            {
                var invalidSecurityStamp = true;

                if (_userManager.SupportsUserSecurityStamp)
                {
                    var stamp = context.Subject?.FindFirstValue("security_stamp");

                    if (stamp != null)
                    {
                        var currentStamp = await _userManager.GetSecurityStampAsync(user);

                        invalidSecurityStamp = (stamp != currentStamp);
                    }
                }

                context.IsActive = !invalidSecurityStamp && !await _userManager.IsLockedOutAsync(user);
            }
        }

        private async Task<List<Claim>> GetClaims(TUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, await _userManager.GetUserIdAsync(user)),
                new Claim(JwtClaimTypes.Name, await _userManager.GetUserNameAsync(user))
            };

            if (_userManager.SupportsUserEmail)
            {
                var email = await _userManager.GetEmailAsync(user);

                if (!String.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(JwtClaimTypes.Email, email));
                    claims.Add(new Claim(JwtClaimTypes.EmailVerified, await _userManager.IsEmailConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean));
                }
            }

            if (_userManager.SupportsUserPhoneNumber)
            {
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

                if (!String.IsNullOrWhiteSpace(phoneNumber))
                {
                    claims.Add(new Claim(JwtClaimTypes.PhoneNumber, phoneNumber));
                    claims.Add(new Claim(JwtClaimTypes.PhoneNumberVerified, await _userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean));
                }
            }

            if (_userManager.SupportsUserClaim)
            {
                claims.AddRange(await _userManager.GetClaimsAsync(user));
            }

            // Roles are included in the claims so no need to return them here.
            //if (_userManager.SupportsUserRole)
            //{
            //    var roles = await _userManager.GetRolesAsync(user);
            //    claims.AddRange(roles.Select(r => new Claim(JwtClaimTypes.Role, r)));
            //}

            return claims;
        }
    }
}
