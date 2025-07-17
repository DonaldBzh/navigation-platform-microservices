using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NavigationPlatform.JourneyService.Application.MappingProfiles;
using NavigationPlatform.Shared.MediatR;
using System.Reflection;

namespace NavigationPlatform.JourneyService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatRServices<IAssemblyMarker>();
        services.AddAutoMapper();
  
        return services;
    }

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        services.AddSingleton(mapperConfig.CreateMapper());
        return services;
    }
}


public interface IAssemblyMarker { }