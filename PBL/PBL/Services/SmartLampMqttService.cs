using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading.Tasks;

namespace PBL.Services
{
    public class SmartLampMqttService
    {
        private readonly IConfiguration _config;

        public SmartLampMqttService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> PublicarAsync(params string[] comandos)
        {
            if (comandos == null || comandos.Length == 0)
                return true;

            var brokerUrl = _config["SmartLampMqtt:BrokerUrl"];
            var topicCmd = _config["SmartLampMqtt:TopicCmd"];

            if (string.IsNullOrWhiteSpace(brokerUrl) || string.IsNullOrWhiteSpace(topicCmd))
                return false;

            var factory = new MqttFactory();
            using var client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("pbl_aquario_" + Guid.NewGuid().ToString("N"))
                .WithWebSocketServer(brokerUrl)
                .Build();

            try
            {
                await client.ConnectAsync(options);

                foreach (var cmd in comandos)
                {
                    if (string.IsNullOrWhiteSpace(cmd))
                        continue;

                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topicCmd)
                        .WithPayload(cmd)
                        .Build();

                    await client.PublishAsync(message);
                }

                await client.DisconnectAsync();
                return true;
            }
            catch
            {
                try { if (client.IsConnected) await client.DisconnectAsync(); } catch { }
                return false;
            }
        }

        public Task<bool> AplicarBrilhoAsync(int brilho)
        {
            var val = Math.Max(0, Math.Min(100, brilho));
            return PublicarAsync("setMode|4", $"setBrightness|{val}");
        }
    }
}
