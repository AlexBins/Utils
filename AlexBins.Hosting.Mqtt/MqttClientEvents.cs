namespace AlexBins.Hosting.Mqtt
{
    using System;
    using MQTTnet;
    using MQTTnet.Client.Connecting;
    using MQTTnet.Client.Disconnecting;
    using MQTTnet.Client.Subscribing;
    using MQTTnet.Protocol;

    /// <summary>
    /// Provides access to raw MQTT events
    /// </summary>
    public sealed class MqttClientEvents
    {
        public event EventHandler<MqttClientConnectedEventArgs> ClientConnected;
        public event EventHandler<MqttClientDisconnectedEventArgs> ClientDisconnected;
        public event EventHandler<MqttApplicationMessageReceivedEventArgs> MessageReceived;
        public event EventHandler<MqttClientSubscribeResultItem> ClientSubscribed;

        internal void InvokeClientConnected(MqttClientConnectedEventArgs args)
        {
            ClientConnected?.Invoke(null, args);
        }

        internal void InvokeClientDisconnected(MqttClientDisconnectedEventArgs args)
        {
            ClientDisconnected?.Invoke(null, args);
        }

        internal void InvokeMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            MessageReceived?.Invoke(null, args);
        }

        internal void InvokeClientSubscribed(MqttClientSubscribeResultItem args)
        {
            ClientSubscribed?.Invoke(null, args);
        }
    }
}
