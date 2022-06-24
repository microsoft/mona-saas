// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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