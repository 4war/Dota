using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dota.Auth.Models;
using Dota.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;

namespace Dota.Auth
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
            AddAuthentication(services);
            services.AddMvc()
                .AddNewtonsoftJson(opts => opts.SerializerSettings
                    .Converters
                    .Add(new StringEnumConverter())
                );
            
            services.AddControllers().AddNewtonsoftJson(x =>
                x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddCors(x => x.AddPolicy("CorsPolicy",
                options => options
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

            services.AddDbContext<AuthContext>(options =>
            {
                options.EnableSensitiveDataLogging();
                options.UseMySQL(
                    Configuration["Data:ConnectionString"]);
            });

            services.AddTransient<Seed>();
            services.AddSingleton(Configuration);
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Seed seed)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("CorsPolicy");
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            SetupEndpoints(app);

            seed.Create();
        }
        
        private void AddAuthentication(IServiceCollection services)
        {
            var authOptionsConfiguration = Configuration.GetSection("Auth");
            var authOptions = authOptionsConfiguration.Get<AuthOptions>();
            services.Configure<AuthOptions>(authOptionsConfiguration);

            var validationForRefreshTokenParameters =  new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = authOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = authOptions.Audience,

                ValidateLifetime = false,

                IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
            };

            services.AddTransient<IEmailService, EmailService>();
            services.AddSingleton(validationForRefreshTokenParameters);
        }
        
        private void SetupEndpoints(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller}/{action}/{id?}");
            });
        }
    }
}