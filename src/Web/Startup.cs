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
            //this.ConfigureAuth(app);
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();
#endif

            var options = new IdentityServerOptions
            {
                SiteName = "AppSyndication Single Sign-On Service",
                SigningCertificate = LoadCertificate(),

                Factory = ConfigureFactory(),

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

        private static X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\idsrv3test.pfx", "idsrv3test");
        }

        private static IdentityServerServiceFactory ConfigureFactory()
        {
            var connectionString = "UseDevelopmentStorage=true;";

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
