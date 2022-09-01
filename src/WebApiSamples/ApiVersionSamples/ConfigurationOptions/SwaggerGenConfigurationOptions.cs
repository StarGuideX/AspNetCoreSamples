using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiVersionSamples.ConfigurationOptions
{
public class SwaggerGenConfigurationOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerGenConfigurationOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var item in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(item.GroupName,
                new OpenApiInfo
                {
                    Title = "ApiVersionSamples",
                    Version = item.ApiVersion.ToString()
                });
        }
    }
}
}
