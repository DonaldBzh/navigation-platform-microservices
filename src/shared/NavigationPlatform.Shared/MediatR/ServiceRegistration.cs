using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NavigationPlatform.Shared.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.Shared.MediatR;

public static class ServiceRegistration
{
    public static IServiceCollection AddMediatRServices<T>(this IServiceCollection services)
    {
        var assembly = typeof(T).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));

        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}

