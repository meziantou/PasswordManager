using System.IO;
using System.Security.Claims;
using Meziantou.PasswordManager.Web.Areas.Api;
using Meziantou.PasswordManager.Web.Areas.Api.Configuration;
using Meziantou.PasswordManager.Web.Areas.Api.Data;
using Meziantou.PasswordManager.Web.Areas.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Meziantou.PasswordManager.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<UserRepository>();
            services.AddSingleton<DocumentRepository>();
            services.AddSingleton<CurrentUserProvider>();



            services.AddSingleton(serviceProvider =>
            {
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
                var db = new PasswordManagerDatabase(Path.Combine(env.ContentRootPath, "db", "PasswordManager.json"));
                db.Load();
                return db;
            });

            var jwtAuthentication = new JwtAuthentication();
            Configuration.GetSection("JwtAuthentication").Bind(jwtAuthentication);
            services.AddSingleton(jwtAuthentication);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.ClaimsIssuer = jwtAuthentication.ValidIssuer;
                    options.IncludeErrorDetails = true;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateActor = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtAuthentication.ValidIssuer,
                        ValidAudience = jwtAuthentication.ValidAudience,
                        IssuerSigningKey = jwtAuthentication.SymmetricSecurityKey,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "areas",
                  template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
    }
}
