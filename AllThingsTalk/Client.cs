using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AllThingsTalk
{
    public interface IClientBase
    {
        Device AttachDevice(string deviceId);
        void PublishAssetState(string deviceId, string assetId, object state);
        event EventHandler<AssetState> OnAssetStateEvent;
        //Asset[] GetAssets(string deviceId);
        //Asset CreateAsset(string deviceId, Asset asset);
        //AssetState GetAssetState(string deviceId, string assetName);
    }

    public class Client : IClientBase
    {
        private readonly Dictionary<string, Device> _devices;
        private MqttClient _mqtt;
        private HttpClient _http;
        private readonly string _apiUri;
        private readonly string _brokerUri;
        private readonly string _token;
        private readonly ILogger _logger;
        private bool _httpError;


        public Client(string token, ILogger logger = null, string apiUri = "http://api.allthingstalk.io", string brokerUri = "api.allthingstalk.io")
        {
            _token = token;
            _brokerUri = brokerUri;
            _apiUri = apiUri;
            _logger = logger;
            _devices = new Dictionary<string, Device>();

            InitMqtt();
            InitHttp();
        }

        public event EventHandler ConnectionReset;

        public event EventHandler<AssetState> OnAssetStateEvent;

        public Device AttachDevice(string deviceId)
        {
            var device = new Device(deviceId);
            _devices[deviceId] = device;
            SubscribeToTopics(deviceId);
            return device;
        }

        public void PublishAssetState(string deviceId, string assetId, object state)
        {
            var toSend = PrepareValueForSendingHTTP(state);
            try
            {
                var uri = "/device/" + deviceId + "/asset/" + assetId + "/state";
                _logger?.Trace("send asset value over HTTP request\nURI: {0}\nvalue: {1}", uri, toSend);

                var request = new HttpRequestMessage(HttpMethod.Put, uri);
                request.Content = new StringContent(toSend, Encoding.UTF8, "application/json");
                request.Headers.Add("Authentication", _token);

                var task = _http.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                using (var result = task.Result)
                {
                    using (var resContent = result.Content)
                    {
                        var contentTask = resContent.ReadAsStringAsync();                                          // ... Read the string.
                        var resultContent = contentTask.Result;
                        _logger?.Trace("send asset value over HTTP response: {0}", resultContent);
                        result.EnsureSuccessStatusCode();
                    }
                }
                _httpError = false;
            }
            catch (Exception e)
            {
                if (_httpError == false && _logger != null)
                {
                    _httpError = true;
                    _logger.Error("HTTP comm problem: {0}", e.ToString());
                }
                if (_logger != null)
                    _logger.Error("failed to send message over http, to: {0}, content: {1}", assetId, toSend);
                else
                    throw;
            }
        }

        /*public Asset[] GetAssets(string deviceId)
        {
       
        }*/

        private void InitMqtt()
        {
            _mqtt = new MqttClient(_brokerUri);
            _mqtt.MqttMsgPublishReceived += OnMqttMsgPublishReceived;
            _mqtt.ConnectionClosed += OnMqttMsgDisconnected;
            var clientId = Guid.NewGuid().ToString().Substring(0, 22);
            _mqtt.Connect(clientId, _token, _token, true, 30);
        }

        private void InitHttp()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(_apiUri)
            };
        }

        private void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                var data = new AssetState(e);

                if (data.State != null || data.Id != null)
                {
                    OnAssetStateEvent?.Invoke(this, data);

                }
                else
                {
                    _logger?.Error("Can't dispatch message");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error("Problem with incomming mqtt message", ex.ToString());
            }
        }

        private void OnMqttMsgDisconnected(object sender, EventArgs e)
        {
            _logger?.Error("mqtt connection lost, recreating...");

            while (_mqtt.IsConnected == false)
            {
                try
                {
                    var clientId = Guid.NewGuid().ToString().Substring(0, 22);
                    _mqtt.Connect(clientId, _token, _token, true, 30);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex.Message);
                }
            }

            _logger?.Trace("mqtt connection recreated, resubscribing...");

            OnConnectionReset();
        }

        private void OnConnectionReset()
        {
            foreach (var device in _devices)
            {
                SubscribeToTopics(device.Key);
            }

            ConnectionReset?.Invoke(this, EventArgs.Empty);
        }

        private void SubscribeToTopics(string deviceId)
        {
            if (_mqtt == null) return;

            lock (_mqtt)
            {
                var toSubscribe = GetTopics(deviceId);
                var qos = new byte[toSubscribe.Length];
                for (var i = 0; i < toSubscribe.Length; i++)
                    qos[i] = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
                _mqtt.Subscribe(toSubscribe, qos);
            }
        }

        private static string[] GetTopics(string deviceId)
        {
            string[] topics = new string[3];
            string root = string.Format("device/{0}", deviceId);
            topics[0] = root + "/asset/+/command";
            topics[1] = root + "/asset/+/event";
            topics[2] = root + "/asset/+/state";
            return topics;
        }

        private string PrepareValueForSendingHTTP(object value)
        {
            string toSend = null;
            JObject result = new JObject();

            result.Add("at", DateTime.UtcNow);
            if (value is JObject)
                result.Add("value", (JObject)value);
            else
            {
                JToken conv;
                try
                {
                    conv = JToken.Parse((string)value);                                         //we need to do this for adding numbers correctly (not as strings, but as numbers)
                }
                catch
                {
                    conv = JToken.FromObject(value);                                            //we need to do this for strings. for some reason, the jtoken parser can't load string values.
                }
                result.Add("value", conv);
            }
            toSend = result.ToString();
            return toSend;
        }
    }
}
