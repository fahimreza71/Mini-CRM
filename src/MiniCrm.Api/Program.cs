using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniCrm.Api.Auth;
using MiniCrm.Api.Endpoints;
using MiniCrm.Api.Errors;
using MiniCrm.Application;
using MiniCrm.Application.Auth;
using MiniCrm.Application.Common;
using MiniCrm.Infrastructure;
using MiniCrm.Infrastructure.Auth;
using MiniCrm.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Default is not configured. Set it with dotnet user-secrets.");

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

// --- who is calling? ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();   // refuse to boot with a missing or weak key

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearer.MapInboundClaims = false;   // keep claim names as written in the token
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],   // only accept the algorithm we use
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = "role"
        };
    });

// --- what may they do? ---
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy(Policies.ManageUsers, p => p.RequireRole(Roles.Admin));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();

    var seedPassword = builder.Configuration["Seed:Password"]
        ?? throw new InvalidOperationException("Seed:Password is not configured. Set it with dotnet user-secrets.");

    using var scope = app.Services.CreateScope();
    await DevDataSeeder.SeedAsync(scope.ServiceProvider, seedPassword);
}

app.UseHttpsRedirection();
app.UseAuthentication();   // who are you?   (must come before)
app.UseAuthorization();    // what may you do?

app.MapHealthChecks("/health").AllowAnonymous();

var v1 = app.NewVersionedApi("MiniCrm")
    .MapGroup("/api/v{version:apiVersion}")
    .HasApiVersion(1.0);

v1.MapAuth();
v1.MapUsers();
v1.MapCustomers();
// v1.MapDeals();   keep your Deals line here

app.Run();

public partial class Program { }