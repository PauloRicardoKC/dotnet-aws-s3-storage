using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Application.Services.Interfaces;
using Storage.Infrastructure.Configuration;
using Storage.Infrastructure.Storage;

namespace Storage.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AwsOptions>()
            .Bind(configuration.GetSection(AwsOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.BucketName), "Aws:BucketName is required.")
            .ValidateOnStart();
        services.AddSingleton<IAmazonS3>(provider =>
            AwsS3StorageProvider.CreateClient(provider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<AwsOptions>>().Value));
        services.AddScoped<IStorageProvider, AwsS3StorageProvider>();

        return services;
    }
}