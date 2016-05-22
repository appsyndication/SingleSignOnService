using System.Collections.Generic;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;

namespace AppSyndication.SingleSignOnService.Web
{
    public static class Clients
    {
        public static List<Client> Get(SsoServiceEnvironmentConfiguration environment)
        {
            var clients = new List<Client>
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "AppSyndication Account Service",
                    ClientId="as-ac",
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret(environment.AccountServiceSecret.Sha256())
                    },

                    Flow = Flows.AuthorizationCode,

                    RedirectUris = new List<string>
                    {
                        "http://www.appsyndication.com/account/signin-oidc",
                        "https://as-ac.azurewebsites.net/account/signin-oidc",
                        "https://localhost:4101/account/signin-oidc",
                        "http://localhost:4001/account/signin-oidc",
                    },

                    //PostLogoutRedirectUris = new List<string>
                    //{
                    //    "https://localhost:44319/logout"
                    //},

                    AllowAccessToAllScopes = true
                },

                new Client
                {
                    Enabled = true,
                    ClientName = "Asp.NET Core Test Client",
                    ClientId="coretest",
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.AuthorizationCode,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44319/signin-oidc",
                    },

                    //PostLogoutRedirectUris = new List<string>
                    //{
                    //    "https://localhost:44319/logout"
                    //},

                    AllowAccessToAllScopes = true
                },

                new Client
                {
                    Enabled = true,
                    ClientName = "AppSyndication Upload Web Service",
                    ClientId = "as-upload-websvc",
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.AuthorizationCode,

                    //AllowedScopes = new List<string>
                    //{
                    //    Constants.StandardScopes.OpenId,
                    //    Constants.StandardScopes.Profile,
                    //    Constants.StandardScopes.Email,
                    //    Constants.StandardScopes.Roles,
                    //    Constants.StandardScopes.OfflineAccess,
                    //},

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44367/signin-oidc",
                        //"https://localhost:44300/cb",
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://localhost:44300/home/contact"
                    },

                    AllowAccessToAllScopes = true
                    //AccessTokenType = AccessTokenType.Reference,
                },

                new Client
                {
                    ClientName = "AppSyndication Console Access",
                    ClientId = "consoleapp",
                    Enabled = true,

                    Flow = Flows.ResourceOwner,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = new List<string> //Scopes.Get().Select(s => s.Name).ToList(),
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Roles,
                        Constants.StandardScopes.OfflineAccess,
                        "upload",
                    },
                },

                new Client
                {
                    ClientName = "AppSyndication Direct Client Access To Upload Service",
                    ClientId = "as-upload-svc",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,

                    Flow = Flows.ClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("B8134623-48DD-E56D-ECD4-AB1F61162CE6".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        "upload"
                    }
                }
            };

            return clients;
        }
    }
}