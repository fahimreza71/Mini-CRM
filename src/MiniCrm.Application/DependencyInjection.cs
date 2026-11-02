using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MiniCrm.Application.Customers;

namespace MiniCrm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<CustomerService>();
        return services;
    }
}
