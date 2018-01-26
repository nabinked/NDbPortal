using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NDbPortal.Command;
using NDbPortal.Names;
using NDbPortal.Query;

namespace NDbPortal
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddNDbPortal(this IServiceCollection services, Action<DbOptions> setupAction)
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
            services.AddScoped<ICommandManager, CommandManager>();
            services.TryAdd(ServiceDescriptor.Scoped(typeof(ITableInfoBuilder<>), typeof(TableInfoBuilder<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(ISqlGenerator<>), typeof(SqlGenerator<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(ICommand<,>), typeof(Command<,>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IQuery<>), typeof(Query<>)));
            services.Configure(setupAction);
            return services;
        }
    }
}
