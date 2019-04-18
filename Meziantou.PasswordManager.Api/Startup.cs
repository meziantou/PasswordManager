using System.IO;
using Meziantou.PasswordManager.Api.Data;
using Meziantou.PasswordManager.Api.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meziantou.PasswordManager.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);

            // Add framework services.
            var builder = services
                .AddMvcCore(options => options.Filters.Add(typeof(PasswordManagerExceptionFilter)))
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1)
                .AddAuthorization()
                .AddFormatterMappings()
                .AddDataAnnotations()
                .AddJsonFormatters();

            // Add application services.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<UserRepository>();
            services.AddSingleton<DocumentRepository>();
            services.AddScoped<PasswordManagerContext>();

            services.AddSingleton(serviceProvider =>
            {
                var env = serviceProvider.GetService<IHostingEnvironment>();
                var logger = serviceProvider.GetService<ILogger<PasswordManagerDatabase>>();
                var path = Configuration.GetValue<string>("PasswordManager:DbPath") ?? "../Meziantou.PasswordManager.json";
                var database = new PasswordManagerDatabase(Path.Combine(env.ContentRootPath, path), logger);
                database.Load();
                return database;
            });

            services.AddSingleton<IBasicAuthenticationUserValidator, PasswordManagerBasicAuthenticationUserValidator>();
            services.AddAuthentication(BasicAuthenticationOptions.AuthenticationScheme)
                .AddBasicAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All,
                RequireHeaderSymmetry = false
            });

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}