using Newtonsoft.Json.Linq;
using System;

namespace AllThingsTalk
{
    public class Asset
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Is { get; set; }
        public Profile Profile { get; set; }
        public AssetState State { get; set; }

        internal Asset(string name)
        {
            this.Name = name;
        }

        internal Asset()
        {

        }

        internal event EventHandler<AssetState> OnPublishState;
        public event EventHandler<AssetState> OnCommand;

        public void PublishState(object value)
        {
            this.State = new AssetState(JToken.FromObject(value));
            State.Id = Name;
            OnPublishState?.Invoke(this, State);
        }

        internal void OnAssetState(AssetState state)
        {
            State = state;
            OnCommand?.Invoke(this, State);
        }
    }

    public class Profile
    {
        public string Type { get; set; }
    }
}
