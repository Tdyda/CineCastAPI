using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using VideoStreamingAPI.Data;
using VideoStreamingAPI.Models;
using VideoStreamingAPI.Repositories;
using VideoStreamingAPI.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text.RegularExpressions;
using System.Collections;

namespace VideoStreamingAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);            

            builder.Services.AddScoped<IMovieRepository, MovieRepository>();

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Configuration.AddEnvironmentVariables();

            var connectionStringTemplate = builder.Configuration.GetConnectionString("ConnectionString");

            var connectionString = Regex.Replace(connectionStringTemplate, @"\$\{(\w+)\}", match =>
            {
                var envVarName = match.Groups[1].Value;
                var envValue = Environment.GetEnvironmentVariable(envVarName);

                if (string.IsNullOrEmpty(envValue))
                {
                    Console.WriteLine($"Brak wartoœci dla zmiennej: {envVarName}");
                    return match.Value;
                }

                return envValue;
            });

            builder.Configuration["ConnectionStrings:ConnectionString"] = connectionString;

            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            builder.Services.AddDbContext<VideoStreamingDbContext>(options =>
                 options.UseMySql(connectionString,
                 new MySqlServerVersion(new Version(10, 11, 6))));

            builder.Services.AddIdentity<AppUserModel, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<VideoStreamingDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IFileRemoveService, FileRemoveService>();
            builder.Services.AddScoped<UserManager<AppUserModel>>();
            builder.Services.AddScoped<SignInManager<AppUserModel>>();
            builder.Services.AddScoped<RoleManager>();
            builder.Services.AddScoped<IServiceProvider, ServiceProvider>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    builder => builder
                    //.WithOrigins("http://localhost:80", "http://localhost:3000")
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
                    //.AllowCredentials());
        });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers()
           .AddJsonOptions(options =>
           {
               options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
           });

            bool isSwaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");
            if (isSwaggerEnabled)
            {
                builder.Services.AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer abcdefgh123456\""
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                });
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_1234567890!@#$%^&*()")),
                    ValidateIssuer = true,
                    ValidIssuer = "YourIssuer",
                    ValidateAudience = true,
                    ValidAudience = "YourAudience",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddSingleton(new TokenService("your_secret_key_1234567890!@#$%^&*()", "YourIssuer", "YourAudience"));

            var app = builder.Build();

            if (app.Environment.IsDevelopment() && isSwaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowReactApp");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
