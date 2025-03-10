using Auth0.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WebAPIProjectFirst.Data;
using WebAPIProjectFirst.Repositories;
using WebAPIProjectFirst.Services;

var builder = WebApplication.CreateBuilder(args);

// Register AppDbContext with the DI container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()  // Allow any origin (or specify a specific one like "http://localhost:3000")
        .AllowAnyMethod()   // Allow any HTTP method (GET, POST, etc.)
        .AllowAnyHeader()); // Allow any headers
});
    
// Register Dapper-based    
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Register TokenService
builder.Services.AddScoped<TokenService>();


builder.Services.AddControllersWithViews();

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        options.Scope = "openid profile email offline_access";
    })
    .WithAccessToken(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
    });
// Add services to the container
builder.Services.AddControllersWithViews();

// Add Swagger services
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Enable Swagger middleware to generate Swagger JSON
app.UseSwagger();
app.UseCors("AllowAll");
// Enable Swagger UI to display Swagger documentation
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger";  // This makes the Swagger UI available at /swagger
});

// Use Routing
app.UseRouting();

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers(); // Use this instead of MapControllerRoute for API controllers

app.Run();
