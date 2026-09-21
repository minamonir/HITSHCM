using Hitshcm.Web.Data;
using Microsoft.AspNetCore.Identity;
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

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var config = provider.GetRequiredService<IConfiguration>();

        await context.Database.EnsureCreatedAsync(cancellationToken);

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = config["Seed:AdminEmail"] ?? DefaultAdminEmail;
        var password = config["Seed:AdminPassword"] ?? DefaultAdminPassword;
        var orgId = config["Seed:OrgId"] ?? DefaultOrgId;
        var bgId = config["Seed:BusinessGroupId"] ?? DefaultBusinessGroupId;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                OrgId = orgId,
                BusinessGroupId = bgId
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to seed admin user: " + string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            logger.LogInformation("Seeded local admin {Email} for org {OrgId} / bg {BgId}.", email, orgId, bgId);
        }
        else
        {
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

            if (dirty)
            {
                await userManager.UpdateAsync(user);
            }
        }

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

        if (await applicationManager.FindByClientIdAsync(clientId, cancellationToken) is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ClientType = ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "HITSHCM Razor BFF",
                RedirectUris =
                {
                    new Uri("http://localhost:5080/callback/login")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:5080/callback/logout")
                },
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
        }
    }
}
