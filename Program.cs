using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OffChainStorage.Services;
using OffChainStorage.Swagger.Filters;
using System.Reflection;
using System.Text;
namespace OffChainStorage.Middlewares
{
   internal class Program
   {
      static Program()
      {
      }

      private static void Main(string[] args)
      {
         var config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();
         var jwtSettings = config.GetSection("JwtSettings");
         var secretKey = jwtSettings.GetValue<string>("SecretKey");
         var issuer = jwtSettings.GetValue<string>("Issuer");
         var audience = jwtSettings.GetValue<string>("Audience");

         var builder = WebApplication.CreateBuilder(args);

         // Add services to the container.

         builder.Services.AddControllers();
         // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
         builder.Services.AddEndpointsApiExplorer();

         builder.Services.AddTransient<Services.IVerifier, VerifierService>();
         builder.Services.AddSwaggerGen(opt =>
         {
            // Configure Swagger documentation
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

            // Configure security definition for bearer token
            opt.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
               Type = SecuritySchemeType.Http,
               BearerFormat = "JWT",
               In = ParameterLocation.Header,
               Scheme = "bearer"
            });

            // Apply authentication filter to operations
            opt.OperationFilter<AuthenticationRequirementsOperationFilter>();

            // Configure XML comments for Swagger documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            opt.IncludeXmlComments(xmlPath);
            opt.EnableAnnotations();
         });

         builder.Services.AddMemoryCache(); // Add memory cache support

         builder.Services.AddCors();
         builder.Services.AddLogging(config =>
         {
            config.AddDebug();
            config.AddConsole();
         });

         builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
              // Configure JWT options
              options.TokenValidationParameters = new TokenValidationParameters
              {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = issuer,
                 ValidAudience = audience,
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
              };
              options.Events = new JwtBearerEvents
              {
                 OnAuthenticationFailed = context =>
                 {
                    ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "Authentication failed");
                    return Task.CompletedTask;
                 },
                 OnTokenValidated = context =>
                 {
                    ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token validated for user: {0}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                 },
              };
           });
         builder.Services.AddAuthorization();

         builder.Services.AddSingleton<IJwtService>(new JwtService(secretKey, issuer, audience));

         var app = builder.Build();

         // Configure the HTTP request pipeline.
         app.UseSwagger(); // Enable Swagger
         app.UseSwaggerUI(c =>
         {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            c.DefaultModelsExpandDepth(-1);
         });
         app.UseCors(builder => builder// Configure UI
                         .AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());

         app.UseHttpsRedirection();
         app.UseAuthentication();
         app.UseAuthorization();
         app.MapControllers();

         app.Run();
      }
   }
}
