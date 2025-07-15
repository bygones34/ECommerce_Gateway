using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Gateway.Handlers;

public class RoleBasedRateLimitHandler : DelegatingHandler
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public RoleBasedRateLimitHandler(IMemoryCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("Authorization"))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = request.Headers.Authorization.Parameter;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        if (roleClaim == "Admin")
        {
            return await base.SendAsync(request, cancellationToken);
        }
        
        var user = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        var cacheKey = $"rate_limit_{user}";
        var currentCount = _cache.GetOrCreate<int>(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        if (currentCount >= 10)
        {
            var response = new HttpResponseMessage((HttpStatusCode)429)
            {
                Content = new StringContent("Too many requests for User role. Please wait.")
            };
            return response;
        }

        _cache.Set(cacheKey, currentCount + 1, TimeSpan.FromMinutes(1));

        return await base.SendAsync(request, cancellationToken);
    }
}