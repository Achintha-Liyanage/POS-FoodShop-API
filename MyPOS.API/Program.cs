using Microsoft.EntityFrameworkCore;
using MyPOS.Domain.Interfaces;
using MyPOS.Infrastructure.Persistence;
using MyPOS.Infrastructure.Persistence.Repositories;
using Serilog; // Added for Serilog
using Serilog.Events; // Added for LogEventLevel

// Configure Serilog logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/myposapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting MyPOS API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for hosting
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
    builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

    // Register Application Services
    using MyPOS.Application.Interfaces;
    using MyPOS.Application.Services;
    using FluentValidation.AspNetCore; // Required for AddFluentValidationAutoValidation
    using FluentValidation; // Required for AddValidatorsFromAssemblyContaining
    using MyPOS.Application.Validators; // Required for CreateProductDtoValidator

    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<IAuthService, AuthService>(); // Register AuthService

    // Register FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    // Configure JWT Authentication & Authorization
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;
    using Microsoft.OpenApi.Models; // For Swagger auth

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
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
    });

    builder.Services.AddAuthorization();

    // Swagger/OpenAPI configuration to include JWT Bearer token
    using System.Reflection; // Required for Assembly.GetExecutingAssembly()
    builder.Services.AddSwaggerGen(c =>
    {
        // Updated API Information
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "MyPOS API", 
            Version = "v1",
            Description = "API for the MyPOS Point of Sale system. Allows managing products, customers, orders, and inventory.",
            TermsOfService = new Uri("https://example.com/terms"), // Replace with actual URI
            Contact = new OpenApiContact
            {
                Name = "MyPOS Support",
                Email = "support@example.com", // Replace with actual email
                Url = new Uri("https://example.com/contact") // Replace with actual URI
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT") // Replace with actual URI
            }
        });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          \r\n\r\nExample: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });

    // Configure Swagger to use XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    // Optionally, include XML comments from the Application project (for DTOs)
    var appXmlFilename = $"{typeof(MyPOS.Application.DTOs.Products.ProductDto).Assembly.GetName().Name}.xml";
    var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFilename);
    if (File.Exists(appXmlPath))
    {
        c.IncludeXmlComments(appXmlPath);
    }

    });
    builder.Services.AddControllers(); // Ensure controllers are added

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

// Global Exception Handler should be early, but after things like HSTS/HTTPS redirection if they might throw.
// And before things that generate responses like MVC/Controllers.
    app.UseHttpsRedirection();

using MyPOS.API.Middleware; // For GlobalExceptionHandlerMiddleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // Register the global exception handler

    app.UseAuthentication(); // Add Authentication middleware
    app.UseAuthorization(); // Add Authorization middleware

    // Remove the default weather forecast endpoint as it's not part of our API
    // var summaries = new[] { ... };
    // app.MapGet("/weatherforecast", () => { ... });

    app.MapControllers(); // Ensure controllers are mapped

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "MyPOS API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Removed WeatherForecast record as it's no longer used
// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) { ... }
