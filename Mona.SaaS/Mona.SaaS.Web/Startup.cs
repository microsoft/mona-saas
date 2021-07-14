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
                o.Instance = "https://login.microsoftonline.com/"; // Static for multi-tenant AAD apps.
                o.ClientId = Configuration["Identity:AppIdentity:AadClientId"];
                o.TenantId = Configuration["common"]; // Static for multi-tenant AAD apps.
                o.CallbackPath = "/signin-oidc";
            });

            // Configuring Mona admin access...

            if (string.IsNullOrEmpty(Configuration["Identity:AdminIdentity:RoleName"])) // Role based auth is the preferred route. If an admin role name is provided, we default to RBAC.
            {
                // By default, any user that belongs to the admin AAD tenant which is automatically configured during the setup process will have access to /admin.

                services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

                services.AddAuthorization(
                    o => o.AddPolicy("admin",
                    p => p.Requirements.Add(new AdminAuthorizationRequirement(
                        Configuration["Identity:AdminIdentity:AadTenantId"],
                        Configuration["Identity:AdminIdentity:AadUserId"],
                        mustBeAdminUser: false)))); // Change this to [true] if *only* the original admin user should have access to /admin.
            }
            else
            {
                // However, if an admin role is specified, we'll use that role and the admin AAD tenant to control access to /admin.

                services.AddSingleton<IAuthorizationHandler, AdminRoleAuthorizationHandler>();

                services.AddAuthorization(
                    o => o.AddPolicy("admin",
                    p => p.Requirements.Add(new AdminRoleAuthorizationRequirement(
                        Configuration["Identity:AdminIdentity:AadTenantId"],
                        Configuration["Identity:AdminIdentity:RoleName"]))));
            }

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
        }

        public void ConfigureOptions(IServiceCollection services)
        {
            services.AddAzureAppConfiguration();

            // Default Configuration Settings Reference
            // =========================================================================
            // NOTE: Many of these settings are automatically configured during setup.
            // -------------------------------------------------------------------------
            // Publisher: PublisherDisplayName                              [Required] *
            // Publisher: PublisherHomePageUrl                              [Optional]
            // Publisher: PublisherPrivacyNoticePageUrl                     [Optional]
            // Publisher: PublisherContactPageUrl                           [Optional]
            // Publisher: PublisherCopyrightNotice                          [Optional]
            // Publisher: SubscriptionConfigurationUrl                      [Required] *
            // Publisher: SubscriptionPurchaseConfirmationUrl               [Required] *

            // Injecting as a regular singleton object that is pre-loaded with values from
            // App Configuration Service but is also mutable upon installation by the
            // setup wizard. This *should* only be needed once right upon installation so 
            // not too worried about concurrency.

            var publisherConfig = new PublisherConfiguration();

            this.Configuration.GetSection("Publisher").Bind(publisherConfig);

            services.AddSingleton(publisherConfig);

            // ---------------------------------------------------------------------------------
            // Deployment: AppInsightsConnectionString                              [Required] *
            // Deployment: AzureSubscriptionId                                      [Required] * 
            // Deployment: AzureResourceGroupName                                   [Required] *
            // Deployment: MarketplaceLandingPageUrl                                [Required] *
            // Deployment: MarketplaceWebhookUrl                                    [Required] *
            // Deployment: InTestMode                                               [Optional]
            // Deployment: Name                                                     [Required] *

            services.Configure<DeploymentConfiguration>(this.Configuration.GetSection("Deployment"));

            // ---------------------------------------------------------------------------------
            // Identity: AppIdentity: AadTenantId                                   [Required] *
            // Identity: AppIdentity: AadClientId                                   [Required] *
            // Identity: AppIdentity: AadClientSecret                               [Required] *
            // Identity: AdminIdentity: AadTenantId                                 [Required] *
            // Identity: AdminIdentity: AadUserId                                   [Required] *
            // Identity: AdminIdentity: RoleName                                    [Optional]

            services.Configure<IdentityConfiguration>(this.Configuration.GetSection("Identity"));

            // ---------------------------------------------------------------------------------
            // Subscriptions: Events: EventGrid: TopicEndpoint                      [Required] *
            // Subscriptions: Events: EventGrid: TopicKey                           [Required] *

            services.Configure<EventGridSubscriptionEventPublisher.Configuration>(this.Configuration.GetSection("Subscriptions:Events:EventGrid"));

            // ---------------------------------------------------------------------------------
            // Subscriptions: Testing: Cache: BlobStorage: ConnectionString         [Required] *
            // Subscriptions: Testing: Cache: BlobStorage: ContainerName            [Optional]

            services.Configure<BlobStorageSubscriptionTestingCache.Configuration>(this.Configuration.GetSection("Subscriptions:Testing:Cache:BlobStorage"));

            // ---------------------------------------------------------------------------------
            // Subscriptions: Staging: Cache: BlobStorage: ConnectionString         [Required] *
            // Subscriptions: Staging: Cache: BlobStorage: ContainerName            [Optional]
            // Subscriptions: Staging: Cache: BlobStorage: TokenExpirationInSeconds [Optional]

            services.Configure<BlobStorageSubscriptionStagingCache.Configuration>(this.Configuration.GetSection("Subscriptions:Staging:Cache:BlobStorage"));
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
            app.UseAzureAppConfiguration();
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