using KogaseEngine.Api.Data;
using KogaseEngine.Core.Services.Auth;
using KogaseEngine.Core.Services.Iam;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Domain.Interfaces.Iam;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Infra.Persistence;
using KogaseEngine.Infra.Repositories.Auth;
using KogaseEngine.Infra.Repositories.Iam;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using KogaseEngine.Core.Services.Telemetry;
using KogaseEngine.Domain.Interfaces.Telemetry;
using KogaseEngine.Infra.Repositories.Telemetry;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();


// Register Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kogase Engine API",
        Version = "v1",
        Description = "API for Kogase Engine"
    });
});


// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Register Repositories
// IAM Module repositories
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

// Auth Module repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();

// Telemetry Module repositories
builder.Services.AddScoped<ITelemetryEventRepository, TelemetryEventRepository>();
builder.Services.AddScoped<IPlaySessionRepository, PlaySessionRepository>();
builder.Services.AddScoped<IMetricAggregateRepository, MetricAggregateRepository>();
builder.Services.AddScoped<IEventDefinitionRepository, EventDefinitionRepository>();


// Register services
// IAM Module services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserRoleService>();

// Auth Module services
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<SessionService>();

// Telemetry Module services
builder.Services.AddScoped<TelemetryEventService>();
builder.Services.AddScoped<PlaySessionService>();
builder.Services.AddScoped<MetricAggregateService>();
builder.Services.AddScoped<EventDefinitionService>();


// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["Secret"] ?? "thisisasecretkeyforjwttokengenerationalgorithmkeyshouldbelonger";
builder.Services.AddScoped<AuthService>(provider => new AuthService(
    provider.GetRequiredService<IAuthTokenRepository>(),
    provider.GetRequiredService<IUserRepository>(),
    provider.GetRequiredService<IDeviceRepository>(),
    provider.GetRequiredService<ISessionRepository>(),
    provider.GetRequiredService<IUnitOfWork>(),
    jwtSecret,
    int.Parse(jwtSettings["TokenExpirationMinutes"] ?? "60"),
    int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7")
));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });


var app = builder.Build();


// Initialize the database
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.Initialize(scope.ServiceProvider);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kogase Engine API v1");
        c.RoutePrefix = string.Empty;
    });
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();