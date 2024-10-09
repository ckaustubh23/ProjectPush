using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using VendorBilling.Application.Common.Interfaces;
using VendorBilling.Infrastructure.Data;
using VendorBilling.Infrastructure.Data.DataAccess;
using VendorBilling.Infrastructure.Repository;

namespace VendorBilling.Extensions
{
    public static class Extensions
    {
        public static void UseUnitOfWork(this IServiceCollection services)
            => services.AddScoped<IUnitOfWork, UnitOfWork>();

        public static void UseDapperDb(this IServiceCollection services)
            => services.AddTransient<ISqlDataAccess, SqlDataAccess>();

        public static void ConfigureSerilog(this IHostBuilder host) =>
            host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

        public static void useDbContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<ApplicationDbContext>(option =>
            {
                option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

        public static void useJWTAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"])),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });
        }

        public static void useSwaggerOptions(this IServiceCollection services) =>
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }
}
