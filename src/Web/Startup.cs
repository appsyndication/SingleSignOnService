using System;
using System.Security.Cryptography.X509Certificates;
using FireGiant.MembershipReboot.AzureStorage;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
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

                Factory = ConfigureFactory(environment),

                AuthenticationOptions =
                {
                    EnableSignOutPrompt = false,
                    EnablePostSignOutAutoRedirect = true,
                },

                EnableWelcomePage = false,

#if DEBUG
                LoggingOptions =
                {
                    EnableHttpLogging = true,
                    EnableKatanaLogging = true,
                    EnableWebApiDiagnostics = true,
                    WebApiDiagnosticsIsVerbose = true,
                },

                RequireSsl = false,
#endif
            };

            app.UseIdentityServer(options);
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

                return (certCollection.Count > 0) ? certCollection[0] : null;
            }
        }

        private static IdentityServerServiceFactory ConfigureFactory(SsoServiceEnvironmentConfiguration environment)
        {
            var connectionString = environment.TableStorageConnectionString;

            var factory = new IdentityServerServiceFactory();

            var scopes = Scopes.Get();

            var scopeStore = new InMemoryScopeStore(scopes);
            factory.ScopeStore = new Registration<IScopeStore>(scopeStore);

            var clients = Clients.Get();

            var clientStore = new InMemoryClientStore(clients);
            factory.ClientStore = new Registration<IClientStore>(clientStore);

            factory.UserService = new Registration<IUserService, UserService>();
            factory.Register(new Registration<AtsUserAccountService>());
            factory.Register(new Registration<AtsUserAccountRepository>());
            factory.Register(new Registration<AtsUserAccountConfig>(r => new AtsUserAccountConfig(connectionString)));

            return factory;
        }
    }

    public class UserService : MembershipRebootUserService<AtsUserAccount>
    {
        public UserService(AtsUserAccountService userService)
            : base(userService)
        {
        }
    }
}
