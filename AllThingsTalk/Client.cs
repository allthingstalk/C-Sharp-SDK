/*
*    _   _ _ _____ _    _              _____     _ _     ___ ___  _  _
*   /_\ | | |_   _| |_ (_)_ _  __ _ __|_   _|_ _| | |__ / __|   \| |/ /
*  / _ \| | | | | | ' \| | ' \/ _` (_-< | |/ _` | | / / \__ \ |) | ' <
* /_/ \_\_|_| |_| |_||_|_|_||_\__, /__/ |_|\__,_|_|_\_\ |___/___/|_|\_\
*                             |___/
*
* Copyright 2018 AllThingsTalk
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AllThingsTalk
{
    public class Client
    {
        private Dictionary<string, Device> Devices { get; }
        private IMqttClient _mqtt;
        private HttpClient _http;
        private IMqttClientOptions _mqttOptions;
        private readonly string _apiUri;
        private readonly string _brokerUri;
        private readonly string _token;
        private readonly ILogger _logger;

        /********** ----- Public methods ----- **********/

        /// <summary>
        /// Client
        /// </summary>
        /// <param name="token">Token from maker</param>
        /// <param name="logger">Logger for receiving messages from SDK.</param>
        /// <param name="apiUri">API url. Default value is "http://api.allthingstalk.io"</param>
        /// <param name="brokerUri">_mqtt url. Default value is "api.allthingstalk.io"</param>
        public Client(string token, ILogger logger = null, string apiUri = "http://api.allthingstalk.io", string brokerUri = "api.allthingstalk.io")
        {
            _token = token;
            _brokerUri = brokerUri;
            _apiUri = apiUri;
            _logger = logger;
            Devices = new Dictionary<string, Device>();

            InitMqtt();
            InitHttp();
        }

        internal event EventHandler ConnectionReset;

        public Device AttachDevice(string deviceId)
        {
            var device = new Device(this, deviceId);
            Devices[deviceId] = device;
            Task.Run(() => GetAssets(deviceId)).Wait();
            Task.Run(() => Subscribe(deviceId)).Wait();
            return device;
        }

        internal void CreateAsset(Device device, Asset asset)
        {
            var jObj = new JObject();
            try
            {
                var jProfile = new JProperty("profile",
                    new JObject(
                        new JProperty("type", asset.Profile.Type.ToLower())));

                jObj.Add("name", asset.Name);
                jObj.Add("is", asset.Is.ToLower());
                jObj.Add(jProfile);

                var httpContent = new StringContent(jObj.ToString(), Encoding.UTF8, "application/json");
                var result = Task.Run(() => _http.PutAsync($"/device/{device.Id}/asset/{asset.Name}", httpContent)).Result;

                using (result)
                {
                    var contentResult = Task.Run(() => result.Content.ReadAsStringAsync()).Result;
                    result.EnsureSuccessStatusCode();
                    var createdAsset = JsonConvert.DeserializeObject<AssetData>(contentResult);
                    _logger?.Trace("New asset {0} created in maker", createdAsset.Name);

                }
            }
            catch (Exception e)
            {
                _logger?.Error("Error creating asset " + e.Message);
            }
        }

        private async Task GetAssets(string deviceId)
        {
            var device = Devices[deviceId];
            if (device == null)
            {
                _logger?.Error("Device doesn't exist.");
                return;
            }

            try
            {
                using (var result = await _http.GetAsync($"/device/{deviceId}/assets"))
                {
                    var stringResult = await result.Content.ReadAsStringAsync();
                    result.EnsureSuccessStatusCode();
                    var assets = JsonConvert.DeserializeObject<List<AssetData>>(stringResult);

                    if (assets.Count == 0)
                        return;

                    foreach (var asset in assets)
                    {
                        _logger?.Trace("New asset {0} added from maker", asset.Name);
                        device.AssetDatas[asset.Name] = asset;
                        device.CreateFromAssetData(asset);
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.Error("Error get assets " + e.Message);
            }
        }

        /********** -----Private methods----- **********/
        /// <summary>
        /// 
        /// </summary>
        private void InitMqtt()
        {
            // TODO: check _mqtt url for any possible errors
            _logger?.Trace("Mqtt initialized");
            var factory = new MqttFactory();
            _mqtt = factory.CreateMqttClient();
            var clientId = Guid.NewGuid().ToString().Substring(0, 22);

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(_brokerUri)
                .WithCredentials(_token, _token)
                .Build();

            Task.Run(() => _mqtt.ConnectAsync(_mqttOptions)).Wait();

            _mqtt.Disconnected += OnMqttDisconnected;
            _mqtt.ApplicationMessageReceived += OnMqttMessageReceived;
            _mqtt.Connected += OnMqttConnected;
        }

        private void InitHttp()
        {
            // TODO: Check http url for any possible errors
            _logger?.Trace("Http initialized");
            var token = $"Bearer {_token}";
            _http = new HttpClient
            {
                BaseAddress = new Uri(_apiUri)
            };

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Add("User-Agent", "ATTalk-C#SDK/6.0.1");
        }

        private void OnMqttMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger?.Trace("Mqtt message received topic: {0}, content: {1}", e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

            try
            {
                var parts = e.ApplicationMessage.Topic.Split('/');
                var deviceId = parts[1];
                var assetName = parts[3];
                var data = new AssetState(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                var device = Devices[deviceId];
                var asset = device.Assets[assetName];

                if (data.State != null && Devices.ContainsKey(deviceId) && device.Assets.ContainsKey(assetName))
                {
                    asset.OnAssetState(data);
                }
                else
                {
                    _logger?.Error("Can't dispatch message");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Problem with incoming Mqtt message", ex.ToString());
            }
        }

        private async void OnMqttConnected(object sender, MqttClientConnectedEventArgs e)
        {
            _logger?.Trace("Mqtt connected");

            foreach (var device in Devices)
            {
                await Subscribe(device.Key);
            }

            ConnectionReset?.Invoke(this, EventArgs.Empty);
        }

        private async void OnMqttDisconnected(object sender, EventArgs e)
        {
            _logger?.Error("Mqtt connection lost, recreating...");

            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _mqtt.ConnectAsync(_mqttOptions);
            }
            catch
            {
                _logger?.Error("Error on Mqtt reconnection");
            }

            _logger?.Trace("Mqtt connection recreated, resubscribing...");
        }

        private async Task Subscribe(string deviceId)
        {
            var topicFilters = new List<TopicFilter>();
            var topics = GetTopics(deviceId);
            topicFilters.AddRange(topics.Select(topic => new TopicFilter(topic, MqttQualityOfServiceLevel.AtMostOnce)));
            await _mqtt.SubscribeAsync(topicFilters.ToArray());

            _logger?.Trace("Mqtt subscribed");
        }

        private static string[] GetTopics(string deviceId)
        {
            var topics = new string[3];
            var root = $"device/{deviceId}";
            topics[0] = root + "/asset/+/command";
            topics[1] = root + "/asset/+/event";
            topics[2] = root + "/asset/+/state";
            return topics;
        }

        internal void PublishAssetState(string deviceId, string name, AssetState state)
        {
            var toSend = JObject.FromObject(state.State).ToString();
            var topic = $"device/{deviceId}/asset/{name}/state";

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(toSend))
                .Build();

            if (_mqtt.IsConnected)
            {
                Task.Run(() => _mqtt.PublishAsync(message)).Wait();
                _logger?.Trace("Message published, topic: {0}, content: {1}", topic, toSend);
            }
            else
            {
                _logger?.Error("Mqtt not connected");
            }
        }
    }
}