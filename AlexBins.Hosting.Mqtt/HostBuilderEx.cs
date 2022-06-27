namespace AlexBins.Hosting.Mqtt
{
    using System;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using MQTTnet;
    using MQTTnet.Adapter;
    using MQTTnet.Client.Options;
    using MQTTnet.Client.Subscribing;
    using MQTTnet.Diagnostics;

    public static class HostBuilderEx
    {
        public static IHostBuilder ConfigureMqtt(this IHostBuilder builder, 
            Action<HostBuilderContext, MqttClientOptionsBuilder, MqttClientSubscribeOptionsBuilder> configureAction)
        {
            return builder.ConfigureServices((ctx, services) =>
            {
                
                services.AddSingleton(provider =>
                {
                    var logger = provider.GetService<IMqttNetLogger>();
                    var adapter = provider.GetService<IMqttClientAdapterFactory>();
                    IMqttFactory factory = provider.GetService<IMqttFactory>() ??
                                           (logger == null ? new MqttFactory() : new MqttFactory(logger));
                    if (logger == null && adapter == null)
                    {
                        return factory.CreateMqttClient();
                    }

                    if (logger == null)
                    {
                        return factory.CreateMqttClient(adapter);
                    }

                    if (adapter == null)
                    {
                        return factory.CreateMqttClient(logger);
                    }

                    return new MqttFactory().CreateMqttClient(logger, adapter);
                });
                services.AddHostedService<MqttService>();
                services.AddSingleton<MqttClientEvents>();

                var optionsBuilder = new MqttClientOptionsBuilder();
                var subscriptionBuilder = new MqttClientSubscribeOptionsBuilder();

                configureAction(ctx, optionsBuilder, subscriptionBuilder);

                services.AddSingleton(optionsBuilder);
                services.AddSingleton(subscriptionBuilder);
            });
        }
    }
}
