using System.Security.Claims;
using System.Text;
using AspNetCoreRateLimit;
using ECommerce.Gateway.Handlers;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TokenBlacklistHandler>();
builder.Services.AddSingleton<RoleBasedRateLimitHandler>();
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//To read Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// builder.Services.AddOcelot();
builder.Services.AddOcelot()
    .AddDelegatingHandler<TokenBlacklistHandler>(true)
    .AddDelegatingHandler<RoleBasedRateLimitHandler>(true);


builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
});

builder.Services.AddHttpClient<TokenBlacklistHandler>(client =>
{
    client.BaseAddress = new Uri("http://authservice-api"); // AuthService URL
});


var app = builder.Build();

// app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.UseOcelot().Wait();

app.Run();