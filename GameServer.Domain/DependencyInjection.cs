using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using MediatR;

using System.Runtime.CompilerServices;
using GameServer.Domain.Decorators;

namespace GameServer.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.Decorate(typeof(IRequestHandler<,>), typeof(AuthDecorator<,>));

            return services;
        }
    }
}
