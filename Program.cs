using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Application.Services;
using ProConnect.Application.Validators;
using ProConnect.Core.Interfaces;
using ProConnect.Infrastructure.Database;
using ProConnect.Infrastructure.Repositores;
using ProConnect.Infrastructure.Services;
using System.Text;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con soporte para JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProConnect API", Version = "v1" });

    // Configuración de autenticación JWT para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuración de MongoDB
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<MongoDbContext>().Database);

// Configuración de HttpClient
builder.Services.AddHttpClient();

// Configuración de JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey");

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Solo para desarrollo
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Registro de dependencias - Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProfessionalProfileRepository, ProfessionalProfileRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();

// Registro de dependencias - Servicios
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfessionalProfileService, ProfessionalProfileService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IProfessionalSearchService, ProfessionalSearchService>();

// Registro de validadores
builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserValidator>();
builder.Services.AddScoped<IValidator<LoginUserDto>, LoginUserValidator>();
builder.Services.AddScoped<IValidator<CreateProfessionalProfileDto>, CreateProfessionalProfileValidator>();
builder.Services.AddScoped<IValidator<UpdateProfessionalProfileDto>, UpdateProfessionalProfileValidator>();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // Agregar orígenes del frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Crear índices de MongoDB al inicio de la aplicación
using (var scope = app.Services.CreateScope())
{
    try
    {
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await mongoContext.CreateIndexesAsync();
        
        // Verificar conexión
        var isConnected = await mongoContext.IsConnectedAsync();
        if (!isConnected)
        {
            Console.WriteLine("Warning: MongoDB is not available. Some features may not work properly.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not initialize MongoDB. Error: {ex.Message}");
        Console.WriteLine("The application will continue but database features may not work.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProConnect API v1");
        c.RoutePrefix = "swagger"; // Ahora Swagger estará en /swagger
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowSpecificOrigins");

// Middleware personalizado para propagar el JWT de la cookie al header Authorization
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwtToken"];
    if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Request.Headers.Add("Authorization", $"Bearer {token}");
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
