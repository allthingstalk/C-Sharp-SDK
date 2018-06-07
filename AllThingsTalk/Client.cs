/*
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

namespace AllThingsTalk
{
    using System;
    using System.Net.Http.Headers;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using System.Threading.Tasks;

    public interface IClientBase
    {
        Device AttachDeviceAsync(string deviceId);
        //AssetState GetAssetState(string deviceId, string assetName);
    }

    public enum SubscriptionType
    {
        Feed,
        Command,
        All
    }

    public class Client : IClientBase
    {
        private Dictionary<string, Device> Devices { get; }
        private MqttClient _mqtt;
        private HttpClient _http;
        private readonly string _apiUri;
        private readonly string _brokerUri;
        private readonly string _token;
        private readonly ILogger _logger;

        /********** -----Public methods----- **********/

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

            Init_mqtt();
            InitHttp();
        }

        internal event EventHandler ConnectionReset;

        public Device AttachDeviceAsync(string deviceId)
        {
            var device = new Device(deviceId);
            Devices[deviceId] = device;
            device.OnPublishState += PublishAssetState;
            device.OnCreateAsset += CreateAsset;
            var topics = GetTopics(deviceId);
            SubscribeToTopics(topics);
            Task.Run(async () => await GetAssets(deviceId));
            return device;
        }

        private void CreateAsset(object deviceObj, Asset asset)
        {
            var device = (Device)deviceObj;

            try
            {
                var jProfile = new JProperty("profile",
                    new JObject(
                        new JProperty("type", asset.Profile.Type.ToLower())));

                var jObj = new JObject();
                jObj.Add("name", asset.Name);
                jObj.Add("is", asset.Is.ToLower());
                jObj.Add(jProfile);

                var httpContent = new StringContent(jObj.ToString(), Encoding.UTF8, "application/json");
                var task = _http.PutAsync($"/device/{device.Id}/asset/{asset.Name}", httpContent);

                using (var result = task.Result)
                {
                    using (var resContent = result.Content)
                    {
                        var contentTask = resContent.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        var createdAsset = JsonConvert.DeserializeObject<Asset>(contentTask.Result);
                        _logger?.Trace("New asset {0} created in maker", createdAsset.Name);
                    }
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
                    var assets = JsonConvert.DeserializeObject<List<Asset>>(stringResult);

                    if (device.Assets.Count == 0)
                        return;

                    foreach (var asset in assets)
                    {
                        _logger?.Trace("New asset {0} added from maker", asset.Name);
                        device.Assets[asset.Name] = asset;
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
        private void Init_mqtt()
        {
            // TODO: check _mqtt url for any possible errors
            _mqtt = new MqttClient(_brokerUri);

            _mqtt.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
            _mqtt.ConnectionClosed += On_mqttMsgDisconnected;
            var clientId = Guid.NewGuid().ToString().Substring(0, 22);
            _mqtt.Connect(clientId, _token, _token, true, 30);
        }

        private void InitHttp()
        {
            // TODO: Check http url for any possible errors
            _logger?.Trace("http initialized");
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

        private void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                var parts = e.Topic.Split(new char[] { '/' });
                var deviceId = parts[1];
                var assetName = parts[3];
                var data = new AssetState(e.Message);
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
                _logger?.Error("Problem with incomming _mqtt message", ex.ToString());
            }
        }

        private void On_mqttMsgDisconnected(object sender, EventArgs e)
        {
            _logger?.Error("_mqtt connection lost, recreating...");

            while (_mqtt.IsConnected == false)
            {
                try
                {
                    var clientId = Guid.NewGuid().ToString().Substring(0, 22);

                    _mqtt.Connect(clientId, _token, _token, true, 30);

                }
                catch (Exception ex)
                {
                    _logger?.Error("Error on _mqtt reconnection" + ex.Message);
                }
            }

            _logger?.Trace("_mqtt connection recreated, resubscribing...");

            OnConnectionReset();
        }

        private void OnConnectionReset()
        {
            foreach (var device in Devices)
            {
                var topics = GetTopics(device.Key);
                SubscribeToTopics(topics);
            }

            ConnectionReset?.Invoke(this, EventArgs.Empty);
        }

        private void SubscribeToTopics(string[] topics)
        {
            if (_mqtt == null) return;

            lock (_mqtt)
            {
                var qos = new byte[topics.Length];
                for (var i = 0; i < topics.Length; i++)
                    qos[i] = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
                _mqtt.Subscribe(topics, qos);
            }
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

        private void PublishAssetState(object device, Asset asset)
        {
            var deviceId = ((Device)device).Id;
            var toSend = JObject.FromObject(asset.State.State).ToString();
            var topic = $"device/{deviceId}/asset/{asset.Name}/state";
            lock (_mqtt)
            {
                _mqtt.Publish(topic, Encoding.UTF8.GetBytes(toSend));
                _logger?.Trace("message published, topic: {0}, content: {1}", topic, toSend);
            }
        }
    }
}
