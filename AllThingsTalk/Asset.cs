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
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;

namespace AllThingsTalk
{
    [DataContract]
    public class AssetData
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

        internal AssetData()
        {
        }
    }

    public abstract class Asset
    {
        public string Name { get; internal set; }
        public string DeviceId { get; internal set; }
        public string Is { get; internal set; }
        public Profile Profile { get; internal set; }
        public AssetState State { get; internal set; }
        public bool Attached { get; internal set; }
        internal Device _device;

        public event EventHandler<Asset> OnCommand;
        internal void SetDevice(Device device)
        {
            _device = device;
        }
        internal void OnAssetState(AssetState state)
        {
            State = state;
            OnCommand?.Invoke(this, this);
        }
    }

    public class Asset<T> : Asset
    {
        internal Asset(Device device, string name, Profile profile, string kind)
        {
            Name = name;
            DeviceId = device.Id;
            Profile = profile;
            Is = kind;
            Attached = false;
            SetDevice(device);
        }

        public void PublishState(T value)
        {
            if (Is == "actuator")
                return;

            State = new AssetState(JToken.FromObject(value));
            _device.Client.PublishAssetState(DeviceId, Name, State);
        }
    }

    public class Profile
    {
        public string Type { get; set; }
    }
}