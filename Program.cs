using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Oracle.ManagedDataAccess.Client;
using OTPManager.Filters;
using OTPManager.Middleware;
using OTPManager.Models;
using OTPManager.Services;
using OTPManager.Services.Interfaces;
using OTPManager.Utilities;
using Serilog;
using System.Net;
using System.Text;

// Add this directive for Serilog console sink

ServicePointManager.Expect100Continue = true;
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure Kestrel to listen on HTTP and HTTPS ports from configuration
builder.WebHost.ConfigureKestrel(options =>
{
    var kestrelConfig = builder.Configuration.GetSection("Kestrel:Endpoints");
    var httpEndpoint = kestrelConfig.GetSection("Http:Url").Value;
    var httpsEndpoint = kestrelConfig.GetSection("Https:Url").Value;

    if (!string.IsNullOrEmpty(httpEndpoint))
    {
        options.Listen(IPAddress.Any, int.Parse(httpEndpoint.Split(':').Last()));
    }
    if (!string.IsNullOrEmpty(httpsEndpoint))
    {
        options.Listen(IPAddress.Any, int.Parse(httpsEndpoint.Split(':').Last()), listenOptions =>
        {
            listenOptions.UseHttps(); // Use HTTPS port
        });
    }
});

// Add services to the container.
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<SendEmailTotp>();
builder.Services.AddScoped<SendSMSTotp>();
builder.Services.AddControllers(options => options.Filters.Add<AuditActionFilter>());
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OneTimeCodeApi", Version = "v1" });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventLog(new Microsoft.Extensions.Logging.EventLog.EventLogSettings
{
    SourceName = "OTP MANAGER",
    LogName = "Application"
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder => builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod());
});

var encryptionKey = builder.Configuration["JwtEncryptionKey"];
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddTransient<EmailService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TOTP", policy => policy.RequireClaim("iss", "www.joopy.co.il/TOTP"));
    options.AddPolicy("Joopy", policy => policy.RequireClaim("iss", "www.joopy.co.il"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "TOTP";
})
.AddJwtBearer("TOTP", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidIssuer = "www.joopy.co.il/TOTP",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey))
    };
})
.AddJwtBearer("Joopy", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidIssuer = "www.joopy.co.il",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IOTPService, OtpService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IEMailService, EmailService>();

var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
builder.Services.AddScoped<OracleConnection>(sp => new OracleConnection(connectionString));

var app = builder.Build();

try
{
    StaticDataStore.LoadData(connectionString);
}
catch
{
    // Handle exceptions if necessary
}

// Ensure Swagger is available in all environments
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OneTimeCodeApi v1"));

// Serve static files (required for Swagger UI)
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ClaimsEnrichmentMiddleware>();
app.UseMiddleware<AuditTrailMiddleware>();

// Add logging middleware to catch and log errors
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred.");
        throw;
    }
});

app.MapControllers();
app.UseCors("AllowSpecificOrigin");

app.Run();
