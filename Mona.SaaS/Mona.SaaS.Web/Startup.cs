// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Services.Default;
using Mona.SaaS.Web.Authorization;

namespace Mona.SaaS.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            configuration = PatchMarketplaceIdentityConfiguration(configuration);

            Configuration = configuration;
        }

        private IConfiguration PatchMarketplaceIdentityConfiguration(IConfiguration configuration)
        {
            // Originally, Mona used one identity (the AppIdentity) to both protect the web app frontend
            // and to contact the Marketplace API. Per GH issue #109, we've made a change where these identities
            // are separate (added MarketplaceIdentity).

            // So we don't break existing Mona users that don't have these new configuration settings, we check
            // right here at the very top to see if MarketplaceIdentity is configured. If it isn't, we set the
            // MarketplaceIdentity to the AppIdentity.

            // This change is here explicitly for backward compatability.

            const string mpIdentityAadTenantId = "Identity:MarketplaceIdentity:AadTenantId";
            const string mpIdentityAadClientId = "Identity:MarketplaceIdentity:AadClientId";
            const string mpIdentityAadClientSecret = "Identity:MarketplaceIdentity:AadClientSecret";

            const string appIdentityAadTenantId = "Identity:AppIdentity:AadTenantId";
            const string appIdentityAadClientId = "Identity:AppIdentity:AadClientId";
            const string appIdentityClientSecret = "Identity:AppIdentity:AadClientSecret";

            configuration[mpIdentityAadTenantId] = configuration.GetValue<string>(
                mpIdentityAadTenantId, // Use the Marketplace tenant ID if configured.
                configuration[appIdentityAadTenantId]); // Patch it with the app tenant ID if not.

            configuration[mpIdentityAadClientId] = configuration.GetValue<string>(
                mpIdentityAadClientId, // Use the Marketplace client ID if configured.
                configuration[appIdentityAadClientId]); // Patch it with the app client ID if not.

            configuration[mpIdentityAadClientSecret] = configuration.GetValue<string>(
                mpIdentityAadClientSecret, // Use the Marketplace client secret if configured.
                configuration[appIdentityClientSecret]); // Patch it with the app client secret if not.

            return configuration;
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
                .AddMicrosoftIdentityUI()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

            services.AddApplicationInsightsTelemetry(Configuration["Deployment:AppInsightsInstrumentationKey"]);
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(o =>
                {
                    o.Instance = "https://login.microsoftonline.com";
                    o.ClientId = Configuration["Identity:AppIdentity:AadClientId"];
                    o.ClientSecret = Configuration["Identity:AppIdentity:AadClientSecret"];
                    o.TenantId = "common"; // Static for multi-tenant AAD apps.
                    o.CallbackPath = "/signin-oidc";
                    o.SignedOutCallbackPath = "/signout-callback-oidc";
                    
                })
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(o =>
                {
                    o.Scopes = "user.read profile";
                    o.BaseUrl = "https://graph.microsoft.com/v1.0";
                })
                .AddInMemoryTokenCaches();

            //Access denied path for when guest account or personal account is used
            services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme,
            options => options.AccessDeniedPath = "/Errors/ErrorAccessDenied");

            
            // Configuring Mona admin access...

            services.AddSingleton<IAuthorizationHandler, AdminRoleAuthorizationHandler>();

            services.AddAuthorization(o =>
            {
                o.FallbackPolicy = o.DefaultPolicy;

                o.AddPolicy("admin",
                    p => p.Requirements.Add(new AdminRoleAuthorizationRequirement(
                    Configuration["Identity:AdminIdentity:AadTenantId"],
                    Configuration["Identity:AdminIdentity:RoleName"])));
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