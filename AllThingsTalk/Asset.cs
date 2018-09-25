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
            Name = name;
            DeviceId = deviceId;
            Profile = profile;
            Is = kind;
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
            var rightType = true;
            var type = value.GetType();
            Console.WriteLine(Profile.Type);

            switch (this.Profile.Type)
            {
                case "integer":
                    if (type != typeof(int))
                        rightType = false;
                    break;
                case "boolean":
                    if (type != typeof(bool))
                        rightType = false;
                    break;
                case "object":
                    if (type != typeof(object))
                        rightType = false;
                    break;
                case "string":
                    if (type != typeof(string))
                        rightType = false;
                    break;
                case "number":
                    if (type != typeof(double))
                        rightType = false;
                    break;       
            }

            if(!rightType)
                throw new ArgumentException("Value type is not correct for this asset!");

            State = new AssetState(JToken.FromObject(value));
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
