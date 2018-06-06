using System;
using System.Collections.Generic;

namespace AllThingsTalk
{
    public class Device
    {
        public string Id { get; }
        public List<Asset> Assets { get; set; }

        internal Device(string deviceId)
        {
            Id = deviceId;
            Assets = new List<Asset>();
        }

        internal event EventHandler <AssetState> OnPublishState;
        internal event EventHandler <Asset> OnCreateAsset;

        /********** -----Public methods----- **********/

        public Asset CreateSensor<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name);

            asset.OnPublishState += PublishAssetState;
            if (asset.Is == null)
            {
                asset.Is = "Sensor";
                OnCreateAsset?.Invoke(this,asset);
            }

            return asset;
        }

        public Asset CreateActuator<T>(string name)
        {
            return new Asset(name);
        }

        public Asset CreateVirtual<T>(string name)
        {
            return new Asset(name);
        }

        public void Subscribe(SubscriptionType type)
        {
            //_client.SubscribeToTopics(GetTopics());
        }

        private void CreateAsset()
        {

        }

        /********** -----Private methods----- **********/

        private Asset GetAssetFromDevice<T>(string name)
        {
            foreach (var asset in Assets)
            {
                if (asset.Name == name)
                    return asset;
            }

            var newAsset = new Asset(name);

            var profile = new Profile();

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32:
                    profile.Type = "Integer";
                    break;
                case TypeCode.Boolean:
                    profile.Type = "Boolean";
                    break;
                case TypeCode.Object:
                    profile.Type = "Object";
                    break;
                case TypeCode.String:
                    profile.Type = "String";
                    break;
                case TypeCode.Double:
                    profile.Type = "Number";
                    break;
            }

            newAsset.Profile = profile;

            return newAsset;
        }

        private void PublishAssetState(object asset, AssetState state)
        {
            OnPublishState?.Invoke(this, state);
        }

        private string[] GetTopics()
        {
            var topics = new string[3];
            var root = $"device/{Id}";
            topics[0] = root + "/asset/+/command";
            topics[1] = root + "/asset/+/event";
            topics[2] = root + "/asset/+/state";
            return topics;
        }
    }
}
