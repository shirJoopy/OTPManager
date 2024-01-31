using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneTimeCodeApi.Services;
using Oracle.ManagedDataAccess.Client;
using OTPManager.Services;
using OTPManager.Services.Interfaces;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<OracleConnection>(_ =>
 {
     string connectionString = builder.Configuration.GetConnectionString("OracleConnection");
     return new OracleConnection(connectionString);
 });
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSingleton<IOTPService, OtpService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();

// Add other necessary services here
// E.g., Database context, SMS service configuration, etc.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Optionally, enable Swagger here if you're using it
}

app.UseRouting();

// Add any necessary middleware here
// E.g., Authentication, Authorization, etc.

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.Run();
