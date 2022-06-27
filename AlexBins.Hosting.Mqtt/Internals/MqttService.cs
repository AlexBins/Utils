namespace AlexBins.Hosting.Mqtt.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using MQTTnet;
    using MQTTnet.Client;
    using MQTTnet.Client.Connecting;
    using MQTTnet.Client.Disconnecting;
    using MQTTnet.Client.Options;
    using MQTTnet.Client.Receiving;
    using MQTTnet.Client.Subscribing;

    internal sealed class MqttService : IHostedService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly MqttClientSubscribeOptions _subscriptionOptions;
        private readonly IMqttClientOptions _clientOptions;
        private readonly IMqttClient _client;
        private readonly MqttClientEvents _events;

        private CancellationTokenSource _mainTokenSource;

        private readonly List<Task> _tasks = new List<Task>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public MqttService(
            ILogger<MqttService> logger,
            IMqttClient client,
            MqttClientSubscribeOptionsBuilder subscriptionBuilder,
            MqttClientOptionsBuilder optionsBuilder,
            MqttClientEvents events)
        {
            _logger = logger;
            _logger.LogInformation("Creating MQTT service");

            _client = client;
            _subscriptionOptions = subscriptionBuilder.Build();
            _clientOptions = optionsBuilder.Build();
            _events = events;
            _mainTokenSource = new CancellationTokenSource();
        }

        private async Task ScheduleAsync(Func<CancellationToken, Task> execution, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            try
            {
                _tasks.RemoveAll(t =>
                {
                    if (t.IsCompleted && t.Exception != null)
                    {
                        t.Exception.Handle(ex =>
                        {
                            _logger.LogWarning(ex, "An MQTT task failed to execute");

                            return true;
                        });
                        return true;
                    }

                    return false;
                });
                _tasks.Add(execution.Invoke(_mainTokenSource.Token));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT service");

            _client.UseConnectedHandler(OnClientConnectedAsync);
            _client.UseDisconnectedHandler(OnClientDisconnectedAsync);
            _client.UseApplicationMessageReceivedHandler(_events.InvokeMessageReceived);

            await ScheduleAsync(async token => await _client.ConnectAsync(_clientOptions, token), cancellationToken);

            _logger.LogInformation("MQTT service is running now");
        }

        private async Task OnClientDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            _logger.LogInformation("MQTT client disconnected");

            _events.InvokeClientDisconnected(args);

            _mainTokenSource.Cancel();
            _mainTokenSource = new CancellationTokenSource();

            await ScheduleAsync(async token => await _client.ConnectAsync(_clientOptions, token), _mainTokenSource.Token);
        }

        private async Task OnClientConnectedAsync(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation("MQTT client connected");

            _events.InvokeClientConnected(args);

            await ScheduleAsync(async token =>
            {
                MqttClientSubscribeResult response = await _client.SubscribeAsync(_subscriptionOptions, token);
                response.Items.ForEach(_events.InvokeClientSubscribed);
            }, _mainTokenSource.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT service");

            _mainTokenSource.Cancel();

            await Task.Yield();

            try
            {
                Task.WaitAll(_tasks.ToArray(), cancellationToken);
            }
            catch (Exception)
            {
                //Ignore
            }

            _logger.LogInformation("MQTT service terminated");
        }
    }
}
