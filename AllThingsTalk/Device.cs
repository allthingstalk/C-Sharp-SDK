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
    using System.Collections.Generic;

    public class Device
    {
        internal Device(string deviceId)
        {
            Id = deviceId;
            Assets = new Dictionary<string, Asset>();
        }

        public string Id { get; }

        public Dictionary<string, Asset> Assets { get; set; }

        internal event EventHandler<Asset> OnPublishState;
        internal event EventHandler<Asset> OnCreateAsset;

        /********** -----Public methods----- **********/

        public Asset CreateSensor<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name, "sensor");
            return asset;
        }

        public Asset CreateActuator<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name, "actuator");
            return asset;
        }

        public Asset CreateVirtual<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name, "virtual");
            return asset;
        }

        /********** -----Private methods----- **********/

        private Asset GetAssetFromDevice<T>(string name, string kind)
        {
            if (Assets.ContainsKey(name))
                return Assets[name];

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
                default:
                    throw new ArgumentException("Type must be from the list of used types", typeof(T).ToString());
            }

            var newAsset = new Asset(name, Id, profile, kind);

            if (kind != "actuator")
                newAsset.OnPublishState += PublishAssetState;

            Assets[name] = newAsset;
            OnCreateAsset?.Invoke(this, newAsset);
            return newAsset;
        }

        private void PublishAssetState(object assetObj, Asset asset)
        {
            OnPublishState?.Invoke(this, asset);
        }
    }
}
