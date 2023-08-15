using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OffChainStorage.Swagger.Filters
{
   public interface IAuthenticationRequirementsOperationFilter
   {
      void Apply(OpenApiOperation operation, OperationFilterContext context);
   }
}