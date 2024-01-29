using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Oracle.ManagedDataAccess.Client;
using OneTimeCodeApi.Services;

namespace OneTimeCodeApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            // Register OracleConnection as a scoped dependency
            services.AddScoped<OracleConnection>(_ =>
            {
                string connectionString = Configuration.GetConnectionString("OracleConnection");
                return new OracleConnection(connectionString);
            });

            // Register your custom services like VerificationService
            services.AddScoped<VerificationService>();

            // ... (other service registrations, like controllers, Swagger, etc.)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // app.UseSwagger();
                // app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OneTimeCodeApi v1"));
            }

            app.UseRouting();

            // Add any necessary middleware here
            // For example, if you're using authentication, you would add it like this:
            // app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
