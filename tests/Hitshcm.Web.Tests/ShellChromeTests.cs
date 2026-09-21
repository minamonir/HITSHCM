using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Hitshcm.Web.Identity;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hitshcm.Web.Tests;

public sealed class ShellChromeTests : IClassFixture<HitshcmWebFactory>
{
    private readonly HitshcmWebFactory _factory;

    public ShellChromeTests(HitshcmWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_RendersProductChrome_AndAccessibleAuthCard()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("class=\"skip-link\"", html, StringComparison.Ordinal);
        Assert.Contains("class=\"app-header\"", html, StringComparison.Ordinal);
        Assert.Contains("class=\"brand\"", html, StringComparison.Ordinal);
        Assert.Contains("culture-switch", html, StringComparison.Ordinal);
        Assert.Contains("class=\"auth-split\"", html, StringComparison.Ordinal);
        Assert.Contains("class=\"auth-hero", html, StringComparison.Ordinal);
        Assert.Contains("class=\"auth-card\"", html, StringComparison.Ordinal);
        Assert.Contains("Username or email", html, StringComparison.Ordinal);
        Assert.Contains("for=\"Input_UserNameOrEmail\"", html, StringComparison.Ordinal);
        Assert.Contains("for=\"Input_Password\"", html, StringComparison.Ordinal);
        Assert.Contains("for=\"Input_BusinessGroupId\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"Input_BusinessGroupId\"", html, StringComparison.Ordinal);
        Assert.Contains(IdentityDataSeeder.DefaultBusinessGroupId, html, StringComparison.Ordinal);
        Assert.Contains(SeedBusinessGroupCatalog.HrId, html, StringComparison.Ordinal);
        Assert.Contains("Office 365", html, StringComparison.Ordinal);
        Assert.Contains("OKTA", html, StringComparison.Ordinal);
        Assert.Contains("Coming soon", html, StringComparison.Ordinal);
        Assert.Contains("HITS Agentic", html, StringComparison.Ordinal);
        Assert.Contains("css/tokens", html, StringComparison.Ordinal);
        Assert.Contains("css/app", html, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<table", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TokensCss_ContainsHitsTealAndCyan()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/css/tokens.css");
        var css = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("#105D7E", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#2BAAE3", css, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--brand-deep", css, StringComparison.Ordinal);
        Assert.Contains("--brand-accent", css, StringComparison.Ordinal);
        Assert.Contains("rgba(255, 255, 255, 0.2)", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ArabicLogin_UsesRtlBootstrap_AndActiveCultureControl()
    {
        var client = CreateClient();
        var culture = await client.GetAsync("/Culture/Set?culture=ar&returnUrl=/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, culture.StatusCode);

        var login = await client.GetAsync("/Account/Login");
        var html = await login.Content.ReadAsStringAsync();

        Assert.Contains("dir=\"rtl\"", html, StringComparison.Ordinal);
        Assert.Contains("lang=\"ar\"", html, StringComparison.Ordinal);
        Assert.Contains("bootstrap.rtl", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("تسجيل الدخول", html, StringComparison.Ordinal);
        Assert.Contains("مجموعة الأعمال", html, StringComparison.Ordinal);
        Assert.Contains("Office 365", html, StringComparison.Ordinal);
        Assert.Contains("OKTA", html, StringComparison.Ordinal);
        Assert.Contains("culture-switch__opt is-active", html, StringComparison.Ordinal);
        Assert.Contains("hreflang=\"ar\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-current=\"true\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Landing_RendersIdentityStrip_AndWhatsNextCards()
    {
        var client = CreateClient();
        await SignInAsync(client);

        var home = await client.GetAsync("/");
        var html = await home.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Contains("identity-strip", html, StringComparison.Ordinal);
        Assert.Contains(IdentityDataSeeder.DefaultAdminEmail, html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(IdentityDataSeeder.DefaultOrgId, html, StringComparison.Ordinal);
        Assert.Contains(IdentityDataSeeder.DefaultBusinessGroupId, html, StringComparison.Ordinal);
        Assert.Contains("class=\"next-card\"", html, StringComparison.Ordinal);
        Assert.Contains("Agenda bridge", html, StringComparison.Ordinal);
        Assert.Contains("First slice", html, StringComparison.Ordinal);
        Assert.Contains("Tenant factory", html, StringComparison.Ordinal);
        Assert.Contains("user-menu", html, StringComparison.Ordinal);
        Assert.Contains("Log out", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ArabicLanding_KeepsTenantClaims_AndRtlChrome()
    {
        var client = CreateClient();
        await SignInAsync(client);
        var culture = await client.GetAsync("/Culture/Set?culture=ar&returnUrl=/");
        Assert.Equal(HttpStatusCode.Redirect, culture.StatusCode);

        var home = await client.GetAsync("/");
        var html = await home.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Contains("dir=\"rtl\"", html, StringComparison.Ordinal);
        Assert.Contains("bootstrap.rtl", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("مرحباً بعودتك", html, StringComparison.Ordinal);
        Assert.Contains(IdentityDataSeeder.DefaultOrgId, html, StringComparison.Ordinal);
        Assert.Contains("الخطوة التالية", html, StringComparison.Ordinal);
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        return client;
    }

    private static async Task SignInAsync(HttpClient client)
    {
        var loginGet = await client.GetAsync("/Account/Login");
        var html = await loginGet.Content.ReadAsStringAsync();
        var token = GetAntiforgeryToken(html);
        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.DefaultAdminEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
    }

    private static string GetAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            match = Regex.Match(
                html,
                "value=\"([^\"]+)\"[^>]*name=\"__RequestVerificationToken\"",
                RegexOptions.IgnoreCase);
        }

        Assert.True(match.Success, "Could not find antiforgery token in HTML.");
        return match.Groups[1].Value;
    }
}
