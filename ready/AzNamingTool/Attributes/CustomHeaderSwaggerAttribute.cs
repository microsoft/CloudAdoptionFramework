using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureNamingTool.Attributes
{
    public class CustomHeaderSwaggerAttribute : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "APIKey",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }

    }
}
