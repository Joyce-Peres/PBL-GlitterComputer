using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Text.Json;
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

        /// <summary>
        /// Publica um único payload JSON no tópico de comando do ESP32.
        /// Usa TCP puro (porta 1883) — compatível com Mosquitto/FIWARE sem WebSocket.
        /// </summary>
        public async Task<bool> PublicarJsonAsync(string jsonPayload, string topicCmd = null)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload))
                return true;

            var brokerHost = _config["SmartLampMqtt:BrokerHost"];
            var brokerPort = int.TryParse(_config["SmartLampMqtt:BrokerPort"], out var p) ? p : 1883;
            var topicFinal = string.IsNullOrWhiteSpace(topicCmd)
                ? _config["SmartLampMqtt:TopicCmd"]
                : topicCmd;

            if (string.IsNullOrWhiteSpace(brokerHost) || string.IsNullOrWhiteSpace(topicFinal))
                return false;

            var factory = new MqttFactory();
            using var client = factory.CreateMqttClient();

            // TCP puro — NÃO usa WithWebSocketServer
            var options = new MqttClientOptionsBuilder()
                .WithClientId("pbl_aquario_" + Guid.NewGuid().ToString("N"))
                .WithTcpServer(brokerHost, brokerPort)
                .WithCleanSession()
                .Build();

            try
            {
                await client.ConnectAsync(options);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topicFinal)
                    .WithPayload(Encoding.UTF8.GetBytes(jsonPayload))
                    .Build();

                await client.PublishAsync(message);
                await client.DisconnectAsync();
                return true;
            }
            catch
            {
                try { if (client.IsConnected) await client.DisconnectAsync(); } catch { }
                return false;
            }
        }

        /// <summary>
        /// Publica os parâmetros de iluminação no formato JSON esperado pelo ESP32:
        /// {"tipo":"peixe","luz_r":R,"luz_g":G,"luz_b":B,"luz_brilho":brilho}
        /// O brilho do ESP32 vai de 0-255; o campo Brilho do model vai de 0-100,
        /// por isso é remapeado proporcionalmente.
        /// </summary>
        public Task<bool> PublicarLuzAsync(int r, int g, int b, int brilhoPercent, string topicCmd = null)
        {
            // Remapeia 0-100 → 0-255 para corresponder ao campo luz_brilho do ESP32
            var brilho255 = (int)Math.Round(Math.Max(0, Math.Min(100, brilhoPercent)) * 255.0 / 100.0);

            var payload = JsonSerializer.Serialize(new
            {
                tipo       = "peixe",
                luz_r      = Math.Max(0, Math.Min(255, r)),
                luz_g      = Math.Max(0, Math.Min(255, g)),
                luz_b      = Math.Max(0, Math.Min(255, b)),
                luz_brilho = brilho255
            });

            return PublicarJsonAsync(payload, topicCmd);
        }

        // Mantido para retrocompatibilidade — não usado pelo SmartLampController novo
        public Task<bool> AplicarBrilhoAsync(int brilho)
        {
            return PublicarLuzAsync(255, 255, 255, brilho);
        }
    }
}
