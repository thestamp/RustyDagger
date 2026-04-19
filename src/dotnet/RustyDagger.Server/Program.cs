using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RustyDagger.Data;
using RustyDagger.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Database=dragoncourt;Username=dragon;Password=dragon";
builder.Services.AddDbContext<DragonCourtDbContext>(opt =>
    opt.UseNpgsql(connectionString));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "RustyDaggerSuperSecretKeyThatIsLongEnough32!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RustyDagger",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RustyDagger",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// Services
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<HeroService>();

builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// Auto-create database schema (EnsureCreated creates tables directly from the model)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DragonCourtDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Serve Blazor WASM static files (when published together)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
