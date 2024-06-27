using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Mona.SaaS.Core.Interfaces;
using Mona.SaaS.Core.Models.Configuration;
using Mona.SaaS.Services;

// Let's build ourselves an admin app...
var builder = WebApplication.CreateBuilder(args);

// Get the configuration...
var configuration = builder.Configuration;

// Wire up authentication...
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(o =>
                {
                    o.Instance = "https://login.microsoftonline.com/";
                    o.ClientId = configuration["Identity:AppIdentity:AadClientId"];
                    o.TenantId = configuration["Identity:AppIdentity:AadTenantId"];
                    o.CallbackPath = "/signin-oidc";
                });

// Wire up Mona services...
builder.Services.AddTransient<ISubscriptionEventPublisher, EventGridSubscriptionEventPublisher>()
                .AddTransient<ISubscriptionTestingCache, BlobStorageSubscriptionTestingCache>()
                .AddTransient<IPublisherConfigurationStore, BlobStoragePublisherConfigurationStore>();

// Wire up configuration...
builder.Services.Configure<DeploymentConfiguration>(configuration.GetSection("Deployment"))
                .Configure<IdentityConfiguration>(configuration.GetSection("Identity"))
                .Configure<MarketplaceConfiguration>(configuration.GetSection("Marketplace"))
                .Configure<EventGridSubscriptionEventPublisher.Configuration>(configuration.GetSection("Subscriptions:Events:EventGrid"))
                .Configure<BlobStorageSubscriptionTestingCache.Configuration>(configuration.GetSection("Subscriptions:Testing:Cache:BlobStorage"))
                .Configure<BlobStoragePublisherConfigurationStore.Configuration>(configuration.GetSection("PublisherConfig:Store:BlobStorage"));

// Set up MVC and lock everything down...
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Wire up the UI...
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
