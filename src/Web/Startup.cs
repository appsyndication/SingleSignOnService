using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using FireGiant.MembershipReboot.AzureStorage;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.MembershipReboot;
using Microsoft.Owin;
using Owin;
using Serilog;

[assembly: OwinStartup(typeof(AppSyndication.SingleSignOnService.Web.Startup))]

namespace AppSyndication.SingleSignOnService.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Trace.TraceInformation("Starting up.");

#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();
#endif

            var environment = new SsoServiceEnvironmentConfiguration();

            var options = new IdentityServerOptions
            {
                SiteName = "AppSyndication Single Sign-On Service",
                SigningCertificate = LoadCertificate(environment),

                CspOptions = new CspOptions()
                {
                    FontSrc = "https://appsyndication.azureedge.net https://maxcdn.bootstrapcdn.com",
                    ScriptSrc = "https://appsyndication.azureedge.net https://code.jquery.com https://maxcdn.bootstrapcdn.com",
                    StyleSrc = "https://appsyndication.azureedge.net https://maxcdn.bootstrapcdn.com",
                },

                Factory = ConfigureFactory(environment),

                AuthenticationOptions =
                {
                    EnableSignOutPrompt = false,
                    //EnablePostSignOutAutoRedirect = true,
                    //PostSignOutAutoRedirectDelay = 0,
                },

                PublicOrigin = environment.PublicOrigin,
                EnableWelcomePage = false,

#if DEBUG
                LoggingOptions =
                {
                    EnableHttpLogging = true,
                    EnableKatanaLogging = true,
                    EnableWebApiDiagnostics = true,
                    WebApiDiagnosticsIsVerbose = true,
                },
#endif

                RequireSsl = false,
            };

            app.Map("/sso", ssoApp =>
            {
                ssoApp.UseIdentityServer(options);
            });

            app.Run(async context =>
            {
                if (context.Request.Path.Value == "/")
                {
                    await context.Response.WriteAsync(
                        @"<!DOCTYPE html><html><head><meta charset=""utf-8""><meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" /><meta name=""viewport"" content = ""width=device-width, initial-scale=1.0"" />" +
                        @"<title>AppSyndication Single Sign-on Service</title>" +
                        @"</head>" +
                        @"<body lang=""en""><h1>AppSyndication Single Sign-on Service</h1><a href=""/sso/"">Go here</a></body>" +
                        @"</html>");
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            });

            Trace.TraceInformation("Started.");
        }

        private static X509Certificate2 LoadCertificate(SsoServiceEnvironmentConfiguration environment)
        {
            if (environment.Environment == "Dev")
            {
                return new X509Certificate2($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\as-id-dev.pfx-dev", "P@ssw0rd1");
            }

            using (var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);
                var certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, environment.CertificateThumprint, false);

                if (certCollection.Count == 0)
                {
                    Trace.TraceError("Failed to load certificate with thumbprint: {0}", environment.CertificateThumprint);
                    return null;
                }

                return certCollection[0];
            }
        }

        private static IdentityServerServiceFactory ConfigureFactory(SsoServiceEnvironmentConfiguration environment)
        {
            var connectionString = environment.TableStorageConnectionString;

            var factory = new IdentityServerServiceFactory();

            var viewOptions = new DefaultViewServiceOptions();
#if DEBUG
            viewOptions.CacheViews = false;
#endif
            viewOptions.Stylesheets.Add("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css");
            viewOptions.Stylesheets.Add("https://maxcdn.bootstrapcdn.com/font-awesome/4.6.2/css/font-awesome.min.css");
            viewOptions.Stylesheets.Add("https://appsyndication.azureedge.net/assets/css/site.css");
            viewOptions.Scripts.Add("http://code.jquery.com/jquery-1.12.3.min.js");
            viewOptions.Scripts.Add("https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js");
            viewOptions.Scripts.Add("https://appsyndication.azureedge.net/assets/js/site.js");

            factory.ConfigureDefaultViewService(viewOptions);

            var scopes = Scopes.Get();

            var scopeStore = new InMemoryScopeStore(scopes);
            factory.ScopeStore = new Registration<IScopeStore>(scopeStore);

            var clients = Clients.Get(environment);

            var clientStore = new InMemoryClientStore(clients);
            factory.ClientStore = new Registration<IClientStore>(clientStore);

            factory.UserService = new Registration<IUserService, UserService>();
            factory.Register(new Registration<AtsUserService>());
            factory.Register(new Registration<AtsUserRepository>());
            factory.Register(new Registration<AtsUserServiceConfig>(r => new AtsUserServiceConfig(connectionString, "appsyndication")));

            return factory;
        }
    }

    public class UserService : MembershipRebootUserService<AtsUser>
    {
        public UserService(AtsUserService userService)
            : base(userService)
        {
        }
    }
}
