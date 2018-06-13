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
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Asset
    /// </summary>
    [DataContract]
    public class Asset
    {
        [DataMember]
        public string Id { get; internal set; }

        [DataMember]
        public string DeviceId { get; internal set; }

        [DataMember]
        public string Name { get; internal set; }

        [DataMember]
        public string Title { get; internal set; }

        [DataMember]
        public string Description { get; internal set; }

        [DataMember]
        public string Is { get; internal set; }

        [DataMember]
        public Profile Profile { get; internal set; }

        [DataMember]
        public AssetState State { get; internal set; }

        internal Asset(string name, string deviceId, Profile profile, string kind)
        {
            this.Name = name;
            this.DeviceId = deviceId;
            this.Profile = profile;
            this.Is = kind;
        }

        internal Asset()
        {
        }

        internal event EventHandler<Asset> OnPublishState;

        /// <summary>
        /// Event handler for actuator. Returns Device as object sender and Asset.
        /// </summary>
        public event EventHandler<Asset> OnCommand;

        public void PublishState(object value)
        {
            this.State = new AssetState(JToken.FromObject(value));
            OnPublishState?.Invoke(this, this);
        }

        internal void OnAssetState(AssetState state)
        {
            State = state;
            OnCommand?.Invoke(this, this);
        }
    }

    public class Profile
    {
        public string Type { get; set; }
    }
}
