using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Application.Interfaces;
using TaskApi.Infrastructure.Identity;
using TaskApi.Infrastructure.Persistence;
using TaskApi.Infrastructure.Persistence.Repositories;
using TaskApi.Infrastructure.Services;

namespace TaskApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Ensure ApplicationDbContext implements IApplicationDbContext
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
           
            // Repositorios de infraestructura
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Servicios de infraestructura
            services.AddTransient<IDateTime, DateTimeService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITokenValidatorService, TokenValidatorService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddHttpContextAccessor();


            services.AddScoped<ApplicationDbContextInitializer>();
            // Configuración JWT
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

            services.AddAuthentication(options =>
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
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var validator = context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();
                        var result = await validator.ValidateAsync(new Application.DTOs.TokenValidationContext
                        {
                            SecurityToken = context.SecurityToken,
                            Principal = context.Principal
                        });

                        if (!result.IsValid)
                        {
                            context.Fail(result.ErrorMessage);
                        }
                    }
                };
            });



            return services;
        }
    }
}
