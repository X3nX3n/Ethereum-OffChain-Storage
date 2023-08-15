using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace OffChainStorage.Swagger.Filters
{
   /// <summary>
   /// The AuthenticationRequirementsOperationFilter class is a custom filter used in the Swagger documentation generation process of an ASP.NET Core application.
   /// It implements the IOperationFilter interface from the Swashbuckle.AspNetCore.SwaggerGen namespace.
   /// This filter is responsible for adding security requirements (authentication) to Swagger operations based on the presence of the Authorize attribute on methods.
   /// </summary>
   public class AuthenticationRequirementsOperationFilter : IOperationFilter
   {
      public void Apply(OpenApiOperation operation, OperationFilterContext context)
      {
         // Check if the method has the Authorize attribute
         var authorizeAttribute = context.MethodInfo.GetCustomAttribute<AuthorizeAttribute>();
         if (authorizeAttribute != null)
         {
            // Add security requirement only if the method is authorized
            if (operation.Security == null)
               operation.Security = new List<OpenApiSecurityRequirement>();

            var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" } };
            operation.Security.Add(new OpenApiSecurityRequirement
            {
               [scheme] = new List<string>()
            });
         }
      }
   }
}
