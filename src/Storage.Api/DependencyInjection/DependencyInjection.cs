using Microsoft.AspNetCore.Http.Features;
using Storage.Application.Constants;

namespace Storage.Api.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddHealthChecks();
        services.Configure<FormOptions>(x => x.MultipartBodyLengthLimit = StorageConstants.MaximumFileSize);
        return services;
    }
}
