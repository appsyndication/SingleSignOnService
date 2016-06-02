using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AppSyndication.SingleSignOnService.Web.Models;
using AppSyndication.SingleSignOnService.Web.Services;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Core.Services;
using System.IO;
using FireGiant.Identity;
using IdentityServer4.Core.Configuration;
using AppSyndication.SingleSignOnService.Web.Identity;
using AppSyndication.SingleSignOnService.Web.Controllers;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace AppSyndication.SingleSignOnService.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            this.Environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.personal.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        private IConfigurationRoot Configuration { get; }

        private IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ClientStoreConfiguration>(this.Configuration.GetSection("ClientSecrets"));

            services.AddSingleton<IClientStore, ClientStore>();

            services.Configure<IdentityServerOptions>(options =>
            {
                options.SiteName = "AppSyndication";
                options.SigningCertificate = this.LoadCertificate(this.Configuration.GetValue<string>("CertificateThumprint"));
                options.RequireSsl = false;
            });

            services.AddIdentityServer()
                .ConfigureIdentity<ApplicationUser>()
                .AddInMemoryScopes(Scopes.Get());

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
                options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
                options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.PreferredUserName;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                options.User.RequireUniqueEmail = true;
            })
                .AddAzureTableStore(this.Configuration.GetConnectionString("Storage"))
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = true;
                options.LowercaseUrls = true;
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/home/error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.Map("/sso", idsrvapp =>
            {
                var publicOrigin = this.Configuration["PublicOrigin"];

                if (!String.IsNullOrEmpty(publicOrigin))
                {
                    idsrvapp.Use(async (context, next) =>
                    {
                        var origin = new Uri(publicOrigin);
                        context.Request.Scheme = origin.Scheme;
                        context.Request.Host = new HostString(origin.Authority);
                        await next();
                    });
                }

                if (env.IsDevelopment())
                {
                    idsrvapp.UseDeveloperExceptionPage();
                }

                idsrvapp.UseIdentity();

                idsrvapp.UseIdentityServer();

                idsrvapp.UseMvc(routes =>
                {
                    routes.MapRoute("ui", "ui/{controller}/{action=Index}/{id?}", new { area = "sso" });
                    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}", new { area = "sso" });
                });
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute("sso", "sso/ui/{controller}/{action=Index}/{id?}", new { area = "sso" });
                routes.MapRoute("root", "{action=Index}/{id?}", new { controller = nameof(Root) });
            });
        }

        private X509Certificate2 LoadCertificate(string thumbprint)
        {
            if (this.Environment.IsDevelopment())
            {
                return new X509Certificate2(Path.Combine(this.Environment.ContentRootPath, "as-id-dev.pfx-dev"), "P@ssw0rd1");
            }
            else if (String.IsNullOrEmpty(thumbprint))
            {
                throw new ArgumentException("'CertificateThumbprint' must be specified outside of development environments.", nameof(thumbprint));
            }

            using (var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);
                var certificates = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (certificates.Count == 0)
                {
                    Trace.TraceError("Failed to load certificate with thumbprint: {0}", thumbprint);
                    return null;
                }

                return certificates[0];
            }
        }
    }
}
