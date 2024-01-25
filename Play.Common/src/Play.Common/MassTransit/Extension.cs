using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;


namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services)
        {
            services.AddMassTransit(config =>
            {

                config.AddConsumers(Assembly.GetEntryAssembly());

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost");
                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageRetry(retryConfigurator =>
                    {
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                    });
                });
            });

            services.AddHostedService<MassTransitHostedService>();

            return services;
        }
    }
}