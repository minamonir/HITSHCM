using System.Security.Claims;

namespace Hitshcm.Web.Tenancy;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;

    public HttpTenantContext(IHttpContextAccessor http) => _http = http;

    public string? OrgId =>
        _http.HttpContext?.User.FindFirstValue(TenantClaimTypes.OrgId);

    public string? BusinessGroupId =>
        _http.HttpContext?.User.FindFirstValue(TenantClaimTypes.BusinessGroupId);
}
