// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Propely.OrgsApi.Application.Common.Behaviors;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;

namespace Propely.OrgsApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Permission evaluation: concrete evaluator + cached decorator
        services.AddScoped<PermissionEvaluator>();
        services.AddScoped<CachedPermissionEvaluator>();
        services.AddScoped<IPermissionEvaluator>(sp => sp.GetRequiredService<CachedPermissionEvaluator>());

        return services;
    }
}
