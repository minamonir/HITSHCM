using System.Security.Claims;

namespace Hitshcm.Web.Tenancy;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;
    private readonly ITenantConnectionFactory _connections;
    private TenantConnectionDescriptor? _descriptor;
    private bool _resolved;

    public HttpTenantContext(IHttpContextAccessor http, ITenantConnectionFactory connections)
    {
        _http = http;
        _connections = connections;
    }

    public string? OrgId =>
        _http.HttpContext?.User.FindFirstValue(TenantClaimTypes.OrgId);

    public string? BusinessGroupId =>
        _http.HttpContext?.User.FindFirstValue(TenantClaimTypes.BusinessGroupId);

    public string? DataSource => Descriptor?.DataSource;

    public string? InitialCatalog => Descriptor?.InitialCatalog;

    private TenantConnectionDescriptor? Descriptor
    {
        get
        {
            if (_resolved)
            {
                return _descriptor;
            }

            _resolved = true;
            var org = OrgId;
            var bg = BusinessGroupId;
            if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(bg))
            {
                return null;
            }

            _descriptor = _connections.Resolve(org, bg);
            return _descriptor;
        }
    }
}
