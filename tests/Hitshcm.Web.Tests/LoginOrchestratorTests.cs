using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Identity;
using Hitshcm.Web.Login;
using Hitshcm.Web.Pages.Account;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hitshcm.Web.Tests;

public sealed class LoginOrchestratorTests : IClassFixture<HitshcmWebFactory>
{
    private readonly HitshcmWebFactory _factory;

    public LoginOrchestratorTests(HitshcmWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void LoginModel_DependsOnLoginOrchestrator()
    {
        var ctor = typeof(LoginModel).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(ILoginOrchestrator));
    }

    [Fact]
    public async Task Authenticate_SeedAdmin_SetsOrgBgAndProfileClaims()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.Succeeded, outcome.Status);
        Assert.Equal(LoginPolicyCodes.Ok, outcome.PolicyCode);
        Assert.True(outcome.ShouldSignIn);
        Assert.NotNull(outcome.Principal);
        Assert.Equal(IdentityDataSeeder.DefaultOrgId, Claim(outcome.Principal, TenantClaimTypes.OrgId));
        Assert.Equal(IdentityDataSeeder.DefaultBusinessGroupId, Claim(outcome.Principal, TenantClaimTypes.BusinessGroupId));
        Assert.Equal("P-ADMIN", Claim(outcome.Principal, TenantClaimTypes.ProfileId));
        Assert.Equal("E-1001", Claim(outcome.Principal, TenantClaimTypes.EmpId));
        Assert.DoesNotContain(outcome.Principal.Claims, c =>
            c.Value.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase)
            || c.Value.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
            || c.Value.Contains("user-server-database", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("ConnectionString", outcome.Principal.Identity?.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Authenticate_SelectedEastGroup_SetsMatchingOrgAndBgClaims()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = SeedBusinessGroupCatalog.EastId
        });

        Assert.Equal(LoginStatus.Succeeded, outcome.Status);
        Assert.Equal(SeedBusinessGroupCatalog.EastOrgId, Claim(outcome.Principal!, TenantClaimTypes.OrgId));
        Assert.Equal(SeedBusinessGroupCatalog.EastId, Claim(outcome.Principal!, TenantClaimTypes.BusinessGroupId));
    }

    [Fact]
    public async Task Authenticate_InactiveUser_Returns002_DoesNotSignIn()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.InactiveEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.PolicyBlocked, outcome.Status);
        Assert.Equal(LoginPolicyCodes.Inactive, outcome.PolicyCode);
        Assert.False(outcome.ShouldSignIn);
        Assert.Equal(LoginPolicyMessages.Inactive, outcome.ErrorMessage);
        Assert.True(await ConsecutiveAttemptsAsync(scope, IdentityDataSeeder.InactiveEmail) >= 1);
    }

    [Fact]
    public async Task Authenticate_MustChangeUser_Returns003_AndChangePasswordPath()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.MustChangeEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.MustChangePassword, outcome.Status);
        Assert.Equal(LoginPolicyCodes.MustChangePassword, outcome.PolicyCode);
        Assert.True(outcome.ShouldSignIn);
        Assert.Equal("/Account/ChangePassword", outcome.RedirectPath);
        Assert.Equal("true", Claim(outcome.Principal!, TenantClaimTypes.MustChangePassword));
    }

    [Fact]
    public async Task Authenticate_HrInactiveUser_Returns004_DoesNotSignIn()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.HrInactiveEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.PolicyBlocked, outcome.Status);
        Assert.Equal(LoginPolicyCodes.HrInactiveForbidden, outcome.PolicyCode);
        Assert.False(outcome.ShouldSignIn);
        Assert.Equal(LoginPolicyMessages.HrInactiveForbidden, outcome.ErrorMessage);
    }

    [Fact]
    public async Task Authenticate_UnknownBusinessGroup_NeedsGroupSelection()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = "not-a-real-bg"
        });

        Assert.Equal(LoginStatus.NeedsBusinessGroup, outcome.Status);
        Assert.False(outcome.ShouldSignIn);
        Assert.Equal(LoginPolicyMessages.SelectBusinessGroup, outcome.ErrorMessage);
    }

    [Fact]
    public async Task Authenticate_MultiBgWithoutSelection_NeedsGroupStepUp()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.MultiBgEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = string.Empty
        });

        Assert.Equal(LoginStatus.NeedsBusinessGroup, outcome.Status);
        Assert.False(outcome.ShouldSignIn);
        Assert.Equal(2, outcome.Memberships.Count);
    }

    [Fact]
    public async Task Authenticate_WrongPassword_IncrementsConsecutiveAttempts()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();
        var before = await ConsecutiveAttemptsAsync(scope, IdentityDataSeeder.DefaultAdminEmail);

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = "WrongPassword!1",
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.InvalidCredentials, outcome.Status);
        Assert.False(outcome.ShouldSignIn);
        Assert.True(await ConsecutiveAttemptsAsync(scope, IdentityDataSeeder.DefaultAdminEmail) > before);
    }

    [Fact]
    public async Task Authenticate_Success_ResetsConsecutiveAttempts()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = "WrongPassword!1",
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.Succeeded, outcome.Status);
        Assert.Equal(0, await ConsecutiveAttemptsAsync(scope, IdentityDataSeeder.DefaultAdminEmail));
    }

    [Fact]
    public async Task ResumeExternal_MatchingSubject_BindsTenantWithoutPassword()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ILoginOrchestrator>();

        var outcome = await orchestrator.ResumeExternalAsync(
            IdentityDataSeeder.DefaultAdminEmail,
            IdentityDataSeeder.DefaultBusinessGroupId,
            IdentityDataSeeder.DefaultAdminEmail);

        Assert.Equal(LoginStatus.Succeeded, outcome.Status);
        Assert.Equal(IdentityDataSeeder.DefaultOrgId, Claim(outcome.Principal!, TenantClaimTypes.OrgId));
    }

    [Fact]
    public async Task PolicyEvaluatorThrow_FailsClosed()
    {
        using var scope = _factory.Services.CreateScope();
        var orchestrator = new LoginOrchestrator(
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<ITenantCatalog>(),
            scope.ServiceProvider.GetRequiredService<ITenantConnectionFactory>(),
            new ThrowingLoginPolicyEvaluator(),
            scope.ServiceProvider.GetRequiredService<ILogger<LoginOrchestrator>>());

        var outcome = await orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = IdentityDataSeeder.DefaultAdminEmail,
            Password = IdentityDataSeeder.DefaultAdminPassword,
            BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId
        });

        Assert.Equal(LoginStatus.FailedClosed, outcome.Status);
        Assert.False(outcome.ShouldSignIn);
        Assert.Equal(LoginPolicyCodes.FailClosed, outcome.PolicyCode);
    }

    [Fact]
    public async Task ConnectionFactory_ReturnsNamedOptions_NotASecret()
    {
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<ITenantConnectionFactory>();

        var descriptor = factory.Resolve(IdentityDataSeeder.DefaultOrgId, IdentityDataSeeder.DefaultBusinessGroupId);

        Assert.Equal("sqlite", descriptor.Provider);
        Assert.Equal("Identity", descriptor.OptionsName);
        Assert.Equal($"HITSHCM-{IdentityDataSeeder.DefaultOrgId}-{IdentityDataSeeder.DefaultBusinessGroupId}", descriptor.ApplicationName);
        Assert.DoesNotContain("Data Source", descriptor.OptionsName, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", descriptor.ApplicationName, StringComparison.OrdinalIgnoreCase);
    }

    private static string Claim(ClaimsPrincipal principal, string type) =>
        principal.FindFirstValue(type) ?? string.Empty;

    private static async Task<int> ConsecutiveAttemptsAsync(IServiceScope scope, string email)
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        return user.ConsecutiveAttempts;
    }

    private sealed class ThrowingLoginPolicyEvaluator : ILoginPolicyEvaluator
    {
        public LoginPolicyResult Evaluate(LoginPolicyInput? input) =>
            throw new InvalidOperationException("policy store unavailable");
    }
}
