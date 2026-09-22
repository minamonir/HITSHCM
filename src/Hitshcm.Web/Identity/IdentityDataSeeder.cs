using Hitshcm.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Hitshcm.Web.Identity;

public static class IdentityDataSeeder
{
    public const string DefaultAdminEmail = "admin@hitshcm.local";
    public const string DefaultAdminPassword = "ChangeMe!123";
    public const string DefaultOrgId = "demo-org";
    public const string DefaultBusinessGroupId = "demo-bg";
    public const string DefaultClientId = "hitshcm-web";
    public const string HitshcmScope = "hitshcm";

    public const string InactiveEmail = "inactive@hitshcm.local";
    public const string MustChangeEmail = "mustchange@hitshcm.local";
    public const string HrInactiveEmail = "hrinactive@hitshcm.local";
    public const string FirstLogonEmail = "firstlogon@hitshcm.local";
    public const string MultiBgEmail = "multibg@hitshcm.local";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var config = provider.GetRequiredService<IConfiguration>();

        await context.Database.EnsureCreatedAsync(cancellationToken);
        await SqliteUserSchemaPatch.ApplyAsync(context, cancellationToken);

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = config["Seed:AdminEmail"] ?? DefaultAdminEmail;
        var password = config["Seed:AdminPassword"] ?? DefaultAdminPassword;
        var orgId = config["Seed:OrgId"] ?? DefaultOrgId;
        var bgId = config["Seed:BusinessGroupId"] ?? DefaultBusinessGroupId;

        await EnsureUserAsync(
            userManager,
            logger,
            email,
            password,
            orgId,
            bgId,
            profileId: "P-ADMIN",
            empId: "E-1001");

        await EnsureUserAsync(
            userManager,
            logger,
            InactiveEmail,
            DefaultAdminPassword,
            orgId,
            bgId,
            profileId: "P-INACTIVE",
            empId: "E-2002",
            inactive: true);

        await EnsureUserAsync(
            userManager,
            logger,
            MustChangeEmail,
            DefaultAdminPassword,
            orgId,
            bgId,
            profileId: "P-MUSTCHG",
            empId: "E-2003",
            mustChangePassword: true);

        await EnsureUserAsync(
            userManager,
            logger,
            HrInactiveEmail,
            DefaultAdminPassword,
            orgId,
            bgId,
            profileId: "P-HRINACT",
            empId: "E-2004",
            employeeHrInactive: true);

        await EnsureUserAsync(
            userManager,
            logger,
            FirstLogonEmail,
            DefaultAdminPassword,
            orgId,
            bgId,
            profileId: "P-FIRST",
            empId: "E-2005",
            firstLogonAckRequired: true);

        await EnsureUserAsync(
            userManager,
            logger,
            MultiBgEmail,
            DefaultAdminPassword,
            orgId,
            bgId,
            profileId: "P-MULTIBG",
            empId: "E-2006");

        var scopeManager = provider.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync(HitshcmScope, cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = HitshcmScope,
                DisplayName = "HITSHCM API",
                Resources = { "hitshcm-api" }
            }, cancellationToken);
        }

        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var clientId = config["OpenIddict:ClientId"] ?? DefaultClientId;
        var clientSecret = config["OpenIddict:ClientSecret"] ?? "dev-only-hitshcm-web-secret";
        var loginHttps = new Uri("https://localhost:3000/callback/login");
        var logoutHttps = new Uri("https://localhost:3000/callback/logout");
        var loginHttp = new Uri("http://localhost:3000/callback/login");
        var logoutHttp = new Uri("http://localhost:3000/callback/logout");

        var application = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        if (application is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ClientType = ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "HITSHCM Razor BFF",
                RedirectUris = { loginHttp, loginHttps },
                PostLogoutRedirectUris = { logoutHttp, logoutHttps },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.Password,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + HitshcmScope
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            }, cancellationToken);

            logger.LogInformation("Seeded OpenIddict client {ClientId}.", clientId);
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, application, cancellationToken);
        if (descriptor.RedirectUris.Contains(loginHttp)
            && descriptor.RedirectUris.Contains(loginHttps)
            && descriptor.PostLogoutRedirectUris.Contains(logoutHttp)
            && descriptor.PostLogoutRedirectUris.Contains(logoutHttps))
        {
            return;
        }

        descriptor.RedirectUris.Clear();
        descriptor.RedirectUris.Add(loginHttp);
        descriptor.RedirectUris.Add(loginHttps);
        descriptor.PostLogoutRedirectUris.Clear();
        descriptor.PostLogoutRedirectUris.Add(logoutHttp);
        descriptor.PostLogoutRedirectUris.Add(logoutHttps);
        await applicationManager.UpdateAsync(application, descriptor, cancellationToken);
        logger.LogInformation("Updated OpenIddict client {ClientId} redirect URIs for HTTP and HTTPS localhost.", clientId);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string password,
        string orgId,
        string bgId,
        string? profileId = null,
        string? empId = null,
        bool inactive = false,
        bool mustChangePassword = false,
        bool employeeHrInactive = false,
        bool firstLogonAckRequired = false)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                OrgId = orgId,
                BusinessGroupId = bgId,
                ProfileId = profileId,
                EmpId = empId,
                Inactive = inactive,
                MustChangePassword = mustChangePassword,
                PasswordDate = DateTimeOffset.UtcNow,
                PasswordNeverExpire = !mustChangePassword,
                EmployeeHrInactive = employeeHrInactive,
                FirstLogonAckRequired = firstLogonAckRequired,
                FirstLogonAckCompleted = false
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed user {email}: " + string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            logger.LogInformation("Seeded local user {Email} for org {OrgId} / bg {BgId}.", email, orgId, bgId);
            return;
        }

        var dirty = false;
        if (string.IsNullOrWhiteSpace(user.OrgId))
        {
            user.OrgId = orgId;
            dirty = true;
        }

        if (string.IsNullOrWhiteSpace(user.BusinessGroupId))
        {
            user.BusinessGroupId = bgId;
            dirty = true;
        }

        if (string.IsNullOrWhiteSpace(user.ProfileId) && profileId is not null)
        {
            user.ProfileId = profileId;
            dirty = true;
        }

        if (string.IsNullOrWhiteSpace(user.EmpId) && empId is not null)
        {
            user.EmpId = empId;
            dirty = true;
        }

        if (dirty)
        {
            await userManager.UpdateAsync(user);
        }
    }
}

/// <summary>
/// EnsureCreated does not add columns to an existing SQLite file. Patch AUTH-1b NasUsers stand-in fields.
/// </summary>
internal static class SqliteUserSchemaPatch
{
    public static async Task ApplyAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (!context.Database.IsSqlite())
        {
            return;
        }

        var columns = new (string Name, string Ddl)[]
        {
            ("ProfileId", "ProfileId TEXT NULL"),
            ("EmpId", "EmpId TEXT NULL"),
            ("Inactive", "Inactive INTEGER NOT NULL DEFAULT 0"),
            ("MustChangePassword", "MustChangePassword INTEGER NOT NULL DEFAULT 0"),
            ("PasswordDate", "PasswordDate TEXT NULL"),
            ("PasswordNeverExpire", "PasswordNeverExpire INTEGER NOT NULL DEFAULT 0"),
            ("WindowsOnly", "WindowsOnly INTEGER NOT NULL DEFAULT 0"),
            ("EmployeeHrInactive", "EmployeeHrInactive INTEGER NOT NULL DEFAULT 0"),
            ("ConsecutiveAttempts", "ConsecutiveAttempts INTEGER NOT NULL DEFAULT 0"),
            ("LastLogonDate", "LastLogonDate TEXT NULL"),
            ("FirstLogonAckRequired", "FirstLogonAckRequired INTEGER NOT NULL DEFAULT 0"),
            ("FirstLogonAckCompleted", "FirstLogonAckCompleted INTEGER NOT NULL DEFAULT 0")
        };

        foreach (var (name, ddl) in columns)
        {
            if (!await ColumnExistsAsync(context, name, cancellationToken))
            {
#pragma warning disable EF1003 // ddl is a hardcoded whitelist of ALTER fragments, not user input.
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE AspNetUsers ADD COLUMN " + ddl,
                    cancellationToken);
#pragma warning restore EF1003
            }
        }
    }

    private static async Task<bool> ColumnExistsAsync(
        ApplicationDbContext context,
        string column,
        CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            var connection = context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('AspNetUsers') WHERE name = $column";
            var columnParam = command.CreateParameter();
            columnParam.ParameterName = "$column";
            columnParam.Value = column;
            command.Parameters.Add(columnParam);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
