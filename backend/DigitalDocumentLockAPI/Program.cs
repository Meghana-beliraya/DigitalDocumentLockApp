using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockRepository.Interfaces;
using DigitalDocumentLockRepository.Repositories;
using DigitalDocumentLockRepository.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YourNamespace.Repositories;
using DigitalDocumentLockCommom.DTOs;
using System.Text;
using DigitalDocumentLockRepository.Services;
using Serilog;
using DigitalDocumentLockAPI.Filters;
using AutoMapper;
using DigitalDocumentLockAPI.Middleware;
using DigitalDocumentLockAPI.Mapping;
using DigitalDocumentLockRepository.UnitOfWork;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

var builder = WebApplication.CreateBuilder(args);

//  Setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Replace default logging

//  Configure Document Encryption Settings
builder.Services.Configure<DocumentEncryptionSettings>(
    builder.Configuration.GetSection("DocumentEncryption"));

//  Database & Repositories
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("DigitalDocumentLockCommon")
    ));





builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IEncryptionService, DocumentEncryptionService>();

//  Inject the updated encryption service
builder.Services.AddScoped<DocumentEncryptionService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISignupRepository, SignupRepository>();
builder.Services.AddScoped<ISignUpService, SignUpService>(); // Optional if directly using SignupRepo
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();


//  JWT Authentication Configuration 
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});

builder.Services.AddAuthorization();

//  Controllers & Swagger 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "1.0.0",
        Title = "Digital Document Lock API",
        Description = "API for Digital Document Lock System"
    });
});

//  CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddScoped<ApiExceptionFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});


// Debug Connection String
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Log.Information("DEBUG: Connection string is: {ConnectionString}", connStr);

//  Build App
var app = builder.Build();

//  Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseStaticFiles();
app.UseCors("AllowAngular");

//  Serilog Middleware for HTTP request logging
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
