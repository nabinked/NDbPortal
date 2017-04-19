using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NDbPortal.Names;

namespace NDbPortal
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDbPortal(this IServiceCollection services, Action<DbOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            services.AddSingleton<INamingConvention, NamingConvention>();
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services.AddSingleton<ICommandFactory, CommandFactory>();
            services.AddSingleton<IStoredProcedure, StoredProcedure>();
            services.TryAdd(ServiceDescriptor.Scoped(typeof(ITableInfoBuilder<>), typeof(TableInfoBuilder<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(Repository<>)));
            services.Configure(setupAction);
            return services;
        }
    }
}
