using Hitshcm.Web.Data;
using Hitshcm.Web.Identity;
using Hitshcm.Web.Infrastructure;
using Hitshcm.Web.Login;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

var isDevHost = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing");

var identityConnection = builder.Configuration.GetConnectionString("Identity")
    ?? "Data Source=App_Data/hitshcm-dev.sqlite";

if (identityConnection.Contains("App_Data", StringComparison.OrdinalIgnoreCase))
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddSingleton<SeedBusinessGroupCatalog>();
builder.Services.AddSingleton<IBusinessGroupCatalog>(sp => sp.GetRequiredService<SeedBusinessGroupCatalog>());
builder.Services.AddSingleton<ITenantCatalog>(sp => sp.GetRequiredService<SeedBusinessGroupCatalog>());
builder.Services.AddSingleton<ITenantConnectionFactory, DevelopmentTenantConnectionFactory>();
builder.Services.AddSingleton<ILoginPolicyEvaluator, LoginPolicyEvaluator>();
builder.Services.AddScoped<ILoginOrchestrator, LoginOrchestrator>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(identityConnection);
    options.UseOpenIddict();
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Lockout.MaxFailedAccessAttempts = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = HitshcmCookieNames.Auth;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = isDevHost ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.ReturnUrlParameter = "ReturnUrl";
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetEndSessionEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo");

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, IdentityDataSeeder.HitshcmScope);

        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow()
            .AllowPasswordFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        var aspNet = options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();

        if (isDevHost)
        {
            aspNet.DisableTransportSecurityRequirement();
        }
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Keep EN/AR copy as real Unicode in markup (RTL smoke, a11y, tests) instead of &#xNNNN; entities.
builder.Services.AddSingleton(HtmlEncoder.Create(
    UnicodeRanges.BasicLatin,
    UnicodeRanges.Latin1Supplement,
    UnicodeRanges.Arabic,
    UnicodeRanges.ArabicSupplement,
    UnicodeRanges.ArabicExtendedA,
    UnicodeRanges.ArabicPresentationFormsA,
    UnicodeRanges.ArabicPresentationFormsB));

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supported = new[] { "en", "fr", "ar" };
    options.SetDefaultCulture("en")
        .AddSupportedCultures(supported)
        .AddSupportedUICultures(supported);

    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider
    {
        CookieName = HitshcmCookieNames.Culture
    });
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
});

builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/ForgotPassword");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/Logout");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Culture/Set");
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

await IdentityDataSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseExceptionHandler("/Error");
}

app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

// D-013b: no app.UseSession() — business state must not live in InProc Session.
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LoginContinuationMiddleware>();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();

public partial class Program
{
    // WebApplicationFactory entry point.
}

// Entra/Okta: add later as OpenIddict external IdPs on this same gateway (D-013).
// After an IdP callback, call ILoginOrchestrator.ResumeExternalAsync (AUTH-1b) — do not bind tenant in the challenge handler.
// Mode A: session-exchange / bridge cookie with legacy NasDna Forms auth is out of scope this slice (D-013b).
