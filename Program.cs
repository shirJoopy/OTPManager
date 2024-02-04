using Oracle.ManagedDataAccess.Client;
using OTPManager.Services.Interfaces;
using OTPManager.Services;
using OTPManager.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IOTPService, OtpService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<OracleConnection>(sp =>
    new OracleConnection(builder.Configuration.GetConnectionString("OracleConnection")));

var app = builder.Build();

app.UseMiddleware<JsonResponse>(); // Add this line

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
