using Microsoft.Extensions.DependencyInjection;
using NavigationPlatform.Shared.MediatR;

namespace NavigationPlatform.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatRServices<IAssemblyMarker>();

        return services;
    }
}



public interface IAssemblyMarker { }