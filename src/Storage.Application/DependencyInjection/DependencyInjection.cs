using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Storage.Application.Services;
using Storage.Application.Services.Interfaces;
using Storage.Application.Storage;

namespace Storage.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IStorageService>();
        services.AddSingleton<StorageKeyBuilder>();
        services.AddScoped<IStorageService, StorageService>();
        return services;
    }
}
