using Hitshcm.Web.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hitshcm.Web.Tests;

public sealed class HitshcmWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"hitshcm-{Guid.NewGuid():N}.sqlite");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={_dbPath}");
        builder.UseSetting("Seed:AdminEmail", IdentityDataSeeder.DefaultAdminEmail);
        builder.UseSetting("Seed:AdminPassword", IdentityDataSeeder.DefaultAdminPassword);
        builder.UseSetting("Seed:OrgId", IdentityDataSeeder.DefaultOrgId);
        builder.UseSetting("Seed:BusinessGroupId", IdentityDataSeeder.DefaultBusinessGroupId);
        builder.UseSetting("OpenIddict:ClientId", IdentityDataSeeder.DefaultClientId);
        builder.UseSetting("OpenIddict:ClientSecret", "test-hitshcm-web-secret");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TryDelete(_dbPath);
        TryDelete(_dbPath + "-shm");
        TryDelete(_dbPath + "-wal");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup of the throwaway SQLite file.
        }
    }
}
