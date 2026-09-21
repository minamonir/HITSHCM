using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Hitshcm.Web.Identity;
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
        Assert.Equal("/Account/Login", response.Headers.Location?.AbsolutePath);
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
            ["ReturnUrl"] = "/",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/", post.Headers.Location?.OriginalString);
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
        Assert.Equal("/Account/Login", logout.Headers.Location?.AbsolutePath);

        var after = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, after.StatusCode);
        Assert.Equal("/Account/Login", after.Headers.Location?.AbsolutePath);
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
