using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NavigationPlatform.Shared.MediatR;

namespace NavigationPlatform.UserManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication( this IServiceCollection services)
    {
        services.AddMediatRServices<IAssemblyMarker>();

        return services;
    }
}

public interface IAssemblyMarker { }