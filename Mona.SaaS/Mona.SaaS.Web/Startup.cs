// MICROSOFT CONFIDENTIAL INFORMATION
//
// Copyright � Microsoft Corporation
//
// Microsoft Corporation (or based on where you live, one of its affiliates) licenses this preview code for your internal testing purposes only.
//
// Microsoft provides the following preview code AS IS without warranty of any kind. The preview code is not supported under any Microsoft standard support program or services.
//
// Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of fitness for a particular purpose. The entire risk arising out of the use or performance of the preview code remains with you.
//
// In no event shall Microsoft be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the preview code, even if Microsoft has been advised of the possibility of such damages.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.EventProcessing.Interfaces;
using Mona.SaaS.Services.Default;
using Mona.SaaS.Web.Authorization;
using System.Threading.Tasks;

namespace Mona.SaaS.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureAuth(services);
            ConfigureDefaultMonaServices(services);
            ConfigureOptions(services);

            services.AddLocalization(o => o.ResourcesPath = "Resources");

            services
                .AddControllersWithViews(o => o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())))
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddNewtonsoftJson();

            services
                .AddRazorPages()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

            services.AddApplicationInsightsTelemetry(Configuration["Deployment:AppInsightsInstrumentationKey"]);
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme).AddAzureAD(o =>
            {
                o.Instance = "https://login.microsoftonline.com/";
                o.ClientId = Configuration["Identity:AppIdentity:AadClientId"];
                o.TenantId = "common"; // Static for multi-tenant AAD apps.
                o.CallbackPath = "/signin-oidc";
            });

            // Configuring Mona admin access...

            services.AddSingleton<IAuthorizationHandler, AdminRoleAuthorizationHandler>();

            services.AddAuthorization(
                o => o.AddPolicy("admin",
                p => p.Requirements.Add(new AdminRoleAuthorizationRequirement(
                    Configuration["Identity:AdminIdentity:AadTenantId"],
                    Configuration["Identity:AdminIdentity:RoleName"]))));

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Instead of using the default validation (validating against a single issuer value, as we do in
                    // line of business apps), we inject our own multitenant validation logic
                    ValidateIssuer = false,

                    // If the app is meant to be accessed by entire organizations, add your issuer validation logic here.
                    //IssuerValidator = (issuer, securityToken, validationParameters) => {
                    //    if (myIssuerValidationLogic(issuer)) return issuer;
                    //}
                };

                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = context =>
                    {
                        // If your authentication logic is based on users then add your logic here
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.Redirect("/Error");
                        context.HandleResponse(); // Suppress the exception
                        return Task.CompletedTask;
                    },
                    // If your application needs to authenticate single users, add your user validation below.
                    //OnTokenValidated = context =>
                    //{
                    //    return myUserValidationLogic(context.Ticket.Principal);
                    //}
                };
            });
        }

        private void ConfigureDefaultMonaServices(IServiceCollection services)
        {
            services.AddTransient<IMarketplaceOperationService, DefaultMarketplaceClient>();
            services.AddTransient<IMarketplaceSubscriptionService, DefaultMarketplaceClient>();
            services.AddTransient<ISubscriptionEventPublisher, EventGridSubscriptionEventPublisher>();
            services.AddTransient<ISubscriptionStagingCache, BlobStorageSubscriptionStagingCache>();
            services.AddTransient<ISubscriptionTestingCache, BlobStorageSubscriptionTestingCache>();
            services.AddTransient<IPublisherConfigurationStore, BlobStoragePublisherConfigurationStore>();
        }

        public void ConfigureOptions(IServiceCollection services)
        {
            services.Configure<DeploymentConfiguration>(this.Configuration.GetSection("Deployment"));
            services.Configure<IdentityConfiguration>(this.Configuration.GetSection("Identity"));
            services.Configure<EventGridSubscriptionEventPublisher.Configuration>(this.Configuration.GetSection("Subscriptions:Events:EventGrid"));
            services.Configure<BlobStorageSubscriptionTestingCache.Configuration>(this.Configuration.GetSection("Subscriptions:Testing:Cache:BlobStorage"));
            services.Configure<BlobStorageSubscriptionStagingCache.Configuration>(this.Configuration.GetSection("Subscriptions:Staging:Cache:BlobStorage"));
            services.Configure<BlobStoragePublisherConfigurationStore.Configuration>(this.Configuration.GetSection("PublisherConfig:Store:BlobStorage"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ConfigureLocalizationMiddleware(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Subscription}/{action=Index}");
                endpoints.MapRazorPages();
            });
        }

        private void ConfigureLocalizationMiddleware(IApplicationBuilder app)
        {
            var supportedCultures = new[] { "en-US", "es" }; // TODO: Support for additional languages coming soon!

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);
        }
    }
}