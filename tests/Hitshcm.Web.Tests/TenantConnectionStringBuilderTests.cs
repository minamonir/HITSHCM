using Hitshcm.Web.Tenancy;

namespace Hitshcm.Web.Tests;

public sealed class TenantConnectionStringBuilderTests
{
    [Fact]
    public void Build_SecurityInfoFalse_UsesIntegratedSecuritySspi()
    {
        var built = TenantConnectionStringBuilder.Build(
            dataSource: "fz-dv-db01",
            initialCatalog: "DNACloudDB",
            securityInfo: "FALSE",
            userId: "ignored",
            password: "should-not-appear",
            applicationName: "HITSHCM-demo-org-demo-bg");

        Assert.Equal(
            "data source=fz-dv-db01;initial catalog=DNACloudDB;integrated security=SSPI;persist security info=False;Application Name=HITSHCM-demo-org-demo-bg;Connection Lifetime=0;packet size=8000;",
            built);
        Assert.DoesNotContain("user id=", built, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("should-not-appear", built, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_SqlAuth_IncludesUserAndPassword()
    {
        var built = TenantConnectionStringBuilder.Build(
            dataSource: "sql-01",
            initialCatalog: "DNACloudDB",
            securityInfo: "TRUE",
            userId: "hits-app",
            password: "plain-from-vault",
            applicationName: "HITSHCM-demo-org-demo-bg",
            persistSecurityInfo: false,
            packetSize: 4096,
            connectionLifetime: 0);

        Assert.Equal(
            "data source=sql-01;initial catalog=DNACloudDB;user id=hits-app;password=plain-from-vault;persist security info=False;Application Name=HITSHCM-demo-org-demo-bg;Connection Lifetime=0;packet size=4096;",
            built);
    }

    [Fact]
    public void Build_MissingServer_Throws()
    {
        Assert.Throws<ArgumentException>(() => TenantConnectionStringBuilder.Build(
            dataSource: " ",
            initialCatalog: "DNACloudDB",
            securityInfo: "FALSE",
            userId: null,
            password: null,
            applicationName: "HITSHCM-x-y"));
    }
}
