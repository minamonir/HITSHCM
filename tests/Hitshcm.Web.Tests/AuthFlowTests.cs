using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Hitshcm.Web.Identity;
using Hitshcm.Web.Login;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hitshcm.Web.Tests;

public sealed class AuthFlowTests : IClassFixture<HitshcmWebFactory>
{
    private readonly HitshcmWebFactory _factory;

    public AuthFlowTests(HitshcmWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousHome_RedirectsToLogin()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", GetLocationPath(response));
    }

    [Fact]
    public async Task OpenIdConfiguration_IsPublished()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("connect/token", body, StringComparison.Ordinal);
        Assert.Contains("connect/authorize", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SeededLogin_IssuesHttpOnlyCookie_AndReachesLanding()
    {
        var client = CreateClient();
        var loginGet = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, loginGet.StatusCode);

        var html = await loginGet.Content.ReadAsStringAsync();
        var token = GetAntiforgeryToken(html);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.DefaultAdminEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["ReturnUrl"] = "/",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/", GetLocationPath(post));
        Assert.Contains(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)
                && v.Contains("httponly", StringComparison.OrdinalIgnoreCase)));

        var home = await client.GetAsync("/");
        var homeHtml = await home.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Contains(IdentityDataSeeder.DefaultAdminEmail, homeHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(IdentityDataSeeder.DefaultOrgId, homeHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", homeHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidPassword_ShowsError_AndDoesNotSetAuthCookie()
    {
        var client = CreateClient();
        var loginGet = await client.GetAsync("/Account/Login");
        var html = await loginGet.Content.ReadAsStringAsync();
        var token = GetAntiforgeryToken(html);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.DefaultAdminEmail,
            ["Input.Password"] = "WrongPassword!1",
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var body = await post.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username/email or password", body, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Logout_ClearsCookie_AndHomeRedirectsToLogin()
    {
        var client = CreateClient();
        await SignInAsync(client);

        var home = await client.GetAsync("/");
        var homeHtml = await home.Content.ReadAsStringAsync();
        var token = GetAntiforgeryToken(homeHtml);

        var logout = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/Account/Login", GetLocationPath(logout));

        var after = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, after.StatusCode);
        Assert.Equal("/Account/Login", GetLocationPath(after));
    }

    [Fact]
    public async Task ArabicCultureStub_SetsRtlOnLogin()
    {
        var client = CreateClient();
        var culture = await client.GetAsync("/Culture/Set?culture=ar&returnUrl=/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, culture.StatusCode);

        var login = await client.GetAsync("/Account/Login");
        var html = await login.Content.ReadAsStringAsync();
        Assert.Contains("dir=\"rtl\"", html, StringComparison.Ordinal);
        Assert.Contains("lang=\"ar\"", html, StringComparison.Ordinal);
        Assert.Contains("مجموعة الأعمال", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_UnknownBusinessGroup_DoesNotIssueCookie()
    {
        var client = CreateClient();
        var loginGet = await client.GetAsync("/Account/Login");
        var html = await loginGet.Content.ReadAsStringAsync();
        var token = GetAntiforgeryToken(html);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.DefaultAdminEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = "not-a-real-bg",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var body = await post.Content.ReadAsStringAsync();
        Assert.Contains("Select a valid business group", body, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task SeededLogin_SelectedBusinessGroup_SetsOrgAndBgClaims()
    {
        var client = CreateClient();
        var loginGet = await client.GetAsync("/Account/Login");
        var html = await loginGet.Content.ReadAsStringAsync();
        var token = GetAntiforgeryToken(html);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.DefaultAdminEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = SeedBusinessGroupCatalog.EastId,
            ["ReturnUrl"] = "/",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Contains(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)
                && v.Contains("httponly", StringComparison.OrdinalIgnoreCase)));

        var home = await client.GetAsync("/");
        var homeHtml = await home.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Contains(SeedBusinessGroupCatalog.EastId, homeHtml, StringComparison.Ordinal);
        Assert.Contains(SeedBusinessGroupCatalog.EastOrgId, homeHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionString", homeHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Data Source=", homeHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=", homeHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_InactiveUser_Shows002_AndDoesNotIssueCookie()
    {
        var client = CreateClient();
        var token = await GetLoginTokenAsync(client);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.InactiveEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var body = await post.Content.ReadAsStringAsync();
        Assert.Contains(LoginPolicyMessages.Inactive, body, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Login_HrInactiveUser_Shows004_AndDoesNotIssueCookie()
    {
        var client = CreateClient();
        var token = await GetLoginTokenAsync(client);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.HrInactiveEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var body = await post.Content.ReadAsStringAsync();
        Assert.Contains(LoginPolicyMessages.HrInactiveForbidden, body, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Login_MustChangeUser_RedirectsToChangePassword()
    {
        var client = CreateClient();
        var token = await GetLoginTokenAsync(client);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.MustChangeEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["ReturnUrl"] = "/",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/Account/ChangePassword", GetLocationPath(post));
        Assert.Contains(post.Headers, h =>
            h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            && h.Value.Any(v => v.Contains("Hitshcm.Auth=", StringComparison.Ordinal)
                && v.Contains("httponly", StringComparison.OrdinalIgnoreCase)));

        var home = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, home.StatusCode);
        Assert.Equal("/Account/ChangePassword", GetLocationPath(home));

        var change = await client.GetAsync("/Account/ChangePassword");
        var changeHtml = await change.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);
        Assert.Contains("Change password", changeHtml, StringComparison.Ordinal);
        Assert.Contains("must be changed", changeHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_FirstLogonUser_RedirectsToAckPlaceholder()
    {
        var client = CreateClient();
        var token = await GetLoginTokenAsync(client);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserNameOrEmail"] = IdentityDataSeeder.FirstLogonEmail,
            ["Input.Password"] = IdentityDataSeeder.DefaultAdminPassword,
            ["Input.BusinessGroupId"] = IdentityDataSeeder.DefaultBusinessGroupId,
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/Account/FirstLogon", GetLocationPath(post));

        var page = await client.GetAsync("/Account/FirstLogon");
        var html = await page.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("First sign-in", html, StringComparison.Ordinal);
    }

    private static async Task<string> GetLoginTokenAsync(HttpClient client)
    {
        var loginGet = await client.GetAsync("/Account/Login");
        var html = await loginGet.Content.ReadAsStringAsync();
        return GetAntiforgeryToken(html);
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

    private static string GetLocationPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var path = location.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString.Split('?')[0];
        return path;
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
