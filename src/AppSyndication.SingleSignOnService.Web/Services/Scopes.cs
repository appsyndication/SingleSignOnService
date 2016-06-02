using System.Collections.Generic;
using IdentityServer4.Core.Models;

namespace AppSyndication.SingleSignOnService.Web.Services
{
    static class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.OfflineAccess,
                StandardScopes.Roles,
                StandardScopes.AllClaims,

                new Scope
                {
                    Name = "upload",
                    DisplayName = "Upload tags",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                }
            };
        }
    }
}