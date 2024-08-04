using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewsAppAPI.Contexts;
using NewsAppAPI.Repositories.Classes;
using NewsAppAPI.Repositories.Interfaces;
using NewsAppAPI.Services.Classes;
using NewsAppAPI.Services.Interfaces;
using System.Text;
using log4net;
using log4net.Config;
using NewsAppAPI.Kafka.Consumers;
using NewsAppAPI.Kafka.Producers;
using NewsAppAPI.Cache;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace NewsAppAPI
{
    public class Program
    {
        private static void AddDbContext(IServiceCollection services, IConfiguration configuration, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        #region RegisterRepositories
        /// <summary>
        /// Registering Repositories
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IReactionRepository, ReactionRepository>();
        }
        #endregion RegisterRepositories

        #region RegisterServices
        /// <summary>
        /// Registering Services
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IReactionService, ReactionService>();

            // Register IMemoryCache
            services.AddMemoryCache();

            // Register ICacheService and other services
            services.AddSingleton<ICacheService, CacheService>();
            //services.AddScoped<IKafkaProducer, KafkaProducer>();

        }
        #endregion RegisterServices

        #region RegisterBackgroundServices
        private static void RegisterBackgroundServices(IServiceCollection services)
        {
            services.AddHostedService<ArticleFetchingService>();
            //services.AddHostedService<CommentConsumer>();
            //services.AddHostedService<ReactionConsumer>();
        }
        #endregion endRegisterBackgroundServices


        #region AddJWTTokenSwaggerGen
        /// <summary>
        /// Adding JWT Token to Swagger
        /// </summary>
        /// <param name="services"></param>
        private static void AddJWTTokenSwaggerGen(IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] { }
                    }
                });
            });
        }
        #endregion AddJWTTokenSwaggerGen

        #region ValidateToken
        private static void ValidateToken(IServiceCollection services, IConfiguration configuration, string JwtToken)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtToken))
                    };

                });
        }
        #endregion ValidateToken

        private static async Task<string> GetSecretAsync(SecretClient secretClient, string secretName)
        {
            try
            {
                KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
                return secret.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
                throw;
            }
        }


        #region ConfigureServices
        /// <summary>
        /// Registering Services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        private static async Task ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(opts =>
            {
                opts.AddPolicy("MyCors", options =>
                {
                    options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpClient();


            //Token operations
            AddJWTTokenSwaggerGen(services);

            // Retrieve secrets from Azure Key Vault

            var keyVaultName = configuration["KeyVault:Name"];
            var kvUri = $"https://{keyVaultName}.vault.azure.net/";

           
            var secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            var jwtToken = await GetSecretAsync(secretClient, "JWT");
            var sqlServerConnectionString = await GetSecretAsync(secretClient, "SqlServerConnectionString");
            var googleClientId = await GetSecretAsync(secretClient, "GoogleClientId");
            var googleClientSecret = await GetSecretAsync(secretClient, "GoogleClientSecret");

            //var sqlServerConnectionString = configuration["ConnectionStrings:DefaultConnection"];
            //var googleClientId = configuration["GoogleAuthSettings:Google:ClientId"];
            //var googleClientSecret = configuration["GoogleAuthSettings:Google:ClientSecret"];

            //await Console.Out.WriteLineAsync(sqlServerConnectionString);
            //await Console.Out.WriteLineAsync(googleClientId);
            //await Console.Out.WriteLineAsync(googleClientSecret);


            if (string.IsNullOrEmpty(sqlServerConnectionString))
            {
                throw new Exception("SqlServerConnectionString cannot be null or empty.");
            }

            ValidateToken(services, configuration, jwtToken);

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = googleClientId;
                googleOptions.ClientSecret = googleClientSecret;
            });


            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            //    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
            //});


            AddDbContext(services, configuration, sqlServerConnectionString);

            RegisterRepositories(services);
            RegisterServices(services);
            RegisterBackgroundServices(services);
        }
        #endregion ConfigureServices

        #region Main
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddLogging(l => l.AddLog4Net());
            // Add services to the container.
            await ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Read the port from configuration
            //var port = builder.Configuration["Kestrel:Endpoints:Http:Url"];
            //app.Urls.Add(port);


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("MyCors");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        #endregion Main
    }
}
