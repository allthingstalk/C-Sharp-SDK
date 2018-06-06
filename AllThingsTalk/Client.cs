using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AllThingsTalk
{
    public interface IClientBase
    {
        Device AttachDevice(string deviceId);
        event EventHandler<AssetState> OnCommand;
        List<Asset> GetAssets(string deviceId);
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
        public Dictionary<string, Device> Devices { get; }
        internal MqttClient Mqtt;
        internal HttpClient Http;
        private readonly string _apiUri;
        private readonly string _brokerUri;
        private readonly string _token;
        internal readonly ILogger Logger;

        /********** -----Public methods----- **********/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="logger"></param>
        /// <param name="apiUri"></param>
        /// <param name="brokerUri"></param>
        public Client(string token, ILogger logger = null, string apiUri = "http://api.allthingstalk.io", string brokerUri = "api.allthingstalk.io")
        {
            _token = token;
            _brokerUri = brokerUri;
            _apiUri = apiUri;
            Logger = logger;
            Devices = new Dictionary<string, Device>();

            InitMqtt();
            InitHttp();
        }

        internal event EventHandler ConnectionReset;

        // TODO: remove this
        public event EventHandler<AssetState> OnCommand;

        public Device AttachDevice(string deviceId)
        {
            var device = new Device(deviceId);
            Devices[deviceId] = device;
            device.OnPublishState += PublishAssetState;
            device.OnCreateAsset += CreateAsset;
            GetAssets(deviceId);
            //SubscribeToTopics(deviceId);
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
                var task = Http.PutAsync($"/device/{device.Id}/asset/{asset.Name}", httpContent);

                using (var result = task.Result)
                {
                    using (var resContent = result.Content)
                    {
                        var contentTask = resContent.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        var createdAsset = JsonConvert.DeserializeObject<Asset>(contentTask.Result);
                        Logger?.Info(createdAsset.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.Error(e.Message);
            }
        }

        public List<Asset> GetAssets(string deviceId)
        {
            var device = Devices[deviceId];
            if (device == null)
            {
                Logger?.Error("Device doesn't exist.");
                return null;
            }

            try
            {
                var task = Http.GetAsync($"/device/{deviceId}/assets");
                using (var result = task.Result)
                {
                    using (var resContent = result.Content)
                    {
                        var contentTask = resContent.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        var assets = JsonConvert.DeserializeObject<List<Asset>>(contentTask.Result);
                        if (device.Assets.Count == 0)
                        {
                            device.Assets = assets;
                        }

                        return assets;
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.Error(e.Message);

            }
            return null;
        }

        /********** -----Private methods----- **********/

        private void InitMqtt()
        {
            Mqtt = new MqttClient(_brokerUri);
            Mqtt.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
            Mqtt.ConnectionClosed += OnMqttMsgDisconnected;
            var clientId = Guid.NewGuid().ToString().Substring(0, 22);
            Mqtt.Connect(clientId, _token, _token, true, 30);
        }

        private void InitHttp()
        {
            var token = $"Bearer {_token}";
            Http = new HttpClient
            {
                BaseAddress = new Uri(_apiUri)
            };

            Http.DefaultRequestHeaders.Accept.Clear();
            Http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Http.DefaultRequestHeaders.Add("User-Agent", "ATTalk-C#SDK/6.0.1");
        }

        private void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                // TODO: parse device and asset here
                var data = new AssetState(e);

                if (data.State != null || data.Id != null)
                {
                    // TODO: Send this to asset
                    OnCommand?.Invoke(this, data);
                }
                else
                {
                    Logger?.Error("Can't dispatch message");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error("Problem with incomming mqtt message", ex.ToString());
            }
        }

        private void OnMqttMsgDisconnected(object sender, EventArgs e)
        {
            Logger?.Error("mqtt connection lost, recreating...");

            while (Mqtt.IsConnected == false)
            {
                try
                {
                    var clientId = Guid.NewGuid().ToString().Substring(0, 22);
                    Mqtt.Connect(clientId, _token, _token, true, 30);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex.Message);
                }
            }

            Logger?.Trace("mqtt connection recreated, resubscribing...");

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

        protected internal void SubscribeToTopics(string[] topics)
        {
            if (Mqtt == null) return;

            lock (Mqtt)
            {
                var qos = new byte[topics.Length];
                for (var i = 0; i < topics.Length; i++)
                    qos[i] = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
                Mqtt.Subscribe(topics, qos);
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

        private void PublishAssetState(object device, AssetState state)
        {
            Logger?.Trace("message published client");
            var deviceId = ((Device)device).Id;
            var toSend = JObject.FromObject(state.State).ToString();
            var topic = $"device/{deviceId}/asset/{state.Id}/state";
            lock (Mqtt)
            {
                Mqtt.Publish(topic, Encoding.UTF8.GetBytes(toSend));
                Logger?.Trace("message published, topic: {0}, content: {1}", topic, toSend);
            }
        }
    }
}
