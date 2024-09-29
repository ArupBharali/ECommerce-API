using ECommerceAPI.ECommerceProductService.Data;
using ECommerceAPI.ECommerceProductService.Models;
using ECommerceProductAPI.Middlewares;
using ECommerceProductAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Determine if running in Docker
var isDocker = builder.Configuration["IsDocker"]?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

// Configure Serilog directly in Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Set the minimum log level
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // Console logging
    .WriteTo.File("logs/ECommerceProductAPILog.log", rollingInterval: RollingInterval.Day) // File logging
    .WriteTo.MSSqlServer(
        connectionString: isDocker? "Server=host.docker.internal;Database=ECommerceProductDB;User Id=sa;Password=interOP@123;TrustServerCertificate=True" : "Server=localhost;Database=ECommerceProductDB;User Id=sa;Password=interOP@123;TrustServerCertificate=True",
        tableName: "SeriLogs",
        autoCreateSqlTable: true) // Database logging
    .CreateLogger(); // Create the logger

// Configure Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"]; // Your Redis server configuration
    options.InstanceName = builder.Configuration["Redis:InstanceName"]; // Optional prefix for cache keys
});

// Use Serilog with configuration from appsettings.json
builder.Host.UseSerilog(); // This will read from appsettings.json

// Add services to the container.
builder.Services.AddDbContext<ECommerceProductDbContext>(options =>
    options.UseSqlServer(isDocker ? builder.Configuration.GetConnectionString("DefaultConnectionDocker") : builder.Configuration.GetConnectionString("DefaultConnectionLocal")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddHttpClient(); // Register HttpClientFactory();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(
    options =>
    {
        options.Events = new ECommerceProductAPI.CustomJwtBearerEvents(builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>());
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    }
    );

// Add authorization services
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/* this is not needed as the token validation is done from the Jwt events */
//app.UseMiddleware<TokenValidationMiddleware>(); // Register the custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Add your custom middleware here

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ECommerceProductDbContext>();
    DbInitializer.InitializeProducts(context);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();
app.UseCors("AllowAllOrigins");


app.UseAuthentication(); // Add this
app.UseAuthorization();  // Add this

app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
    endpoints.MapControllers();
});


app.MapControllers();

// Set Kestrel to listen on port 7208
if (isDocker)
{
    app.Urls.Add("http://*:5268");
}
else
{
    app.Urls.Add("http://*:7268");
}

app.Run();
