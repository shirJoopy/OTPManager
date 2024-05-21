using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Oracle.ManagedDataAccess.Client;
using OTPManager.Filters;
using OTPManager.Middleware; // Ensure this is the correct namespace
using OTPManager.Models;
using OTPManager.Services;
using OTPManager.Services.Interfaces;
using OTPManager.Utilities;
using System.Net;
using System.Text;

ServicePointManager.Expect100Continue = true;
ServicePointManager.SecurityProtocol =
    SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 ;

ServicePointManager.ServerCertificateValidationCallback +=
    (sender, certificate, chain, sslPolicyErrors) => true;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<SendEmailTotp>(); // Register your action filter
builder.Services.AddScoped<SendSMSTotp>();


builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();

});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OneTimeCodeApi", Version = "v1" });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Log to console
builder.Logging.AddDebug();   // Log to debug output
builder.Logging.AddEventLog(new Microsoft.Extensions.Logging.EventLog.EventLogSettings()
{
    SourceName ="OTP MANAGER",
    LogName= "Application"
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("*")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
// Securely retrieve the encryption key
var encryptionKey = builder.Configuration["JwtEncryptionKey"]; // Ensure this is configured in appsettings or as an environment variable

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));

builder.Services.AddTransient<EmailService>();

builder.Services.AddAuthorization(options =>
{
    // Policy for resources accessible by tokens from Issuer A
    options.AddPolicy("TOTP", policy =>
        policy.RequireClaim("iss", "www.joopy.co.il/TOTP"));

    // Policy for resources accessible by tokens from Issuer B
    options.AddPolicy("Joopy", policy =>
        policy.RequireClaim("iss", "www.joopy.co.il"));
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

/*
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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
});
*/

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IOTPService, OtpService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IEMailService, EmailService>();

var connectionString = builder.Configuration.GetConnectionString("OracleConnection");


builder.Services.AddScoped<OracleConnection>(sp =>
    new OracleConnection(connectionString));


var app = builder.Build();

try
{
    StaticDataStore.LoadData(connectionString);
}
catch
{

}
// Load static data here


// Continue with middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OneTimeCodeApi v1"));

// Remaining middleware configurations...
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ClaimsEnrichmentMiddleware>(); // Register the claims enrichment middleware

app.UseMiddleware<AuditTrailMiddleware>();

// Middleware registrations...
app.MapControllers();
app.UseCors("AllowSpecificOrigin");

app.Run();
app.UseMiddleware<JsonResponse>();
