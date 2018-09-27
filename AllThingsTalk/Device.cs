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
using System;
using System.Collections.Generic;

namespace AllThingsTalk
{
    public class Device
    {
        internal Device(Client client, string deviceId)
        {
            Client = client;
            Id = deviceId;
            Assets = new Dictionary<string, Asset>();
            AssetDatas = new Dictionary<string, AssetData>();
        }

        public string Id { get; }

        public readonly Client Client;

        public Dictionary<string, Asset> Assets { get; }
        public Dictionary<string, AssetData> AssetDatas { get; }

        /********** -----Public methods----- **********/
        internal void CreateFromAssetData(AssetData assetData)
        {
            Asset asset;
            switch (assetData.Profile.Type)
            {
                case "integer":
                    asset = GetAssetFromDevice<int>(assetData.Name, assetData.Is);
                    Assets[assetData.Name] = asset;
                    break;
                case "number":
                    asset = GetAssetFromDevice<double>(assetData.Name, assetData.Is);
                    Assets[assetData.Name] = asset;
                    break;
                case "boolean":
                    asset = GetAssetFromDevice<bool>(assetData.Name, assetData.Is);
                    Assets[assetData.Name] = asset;
                    break;
                case "string":
                    asset = GetAssetFromDevice<string>(assetData.Name, assetData.Is);
                    Assets[assetData.Name] = asset;
                    break;
                case "object":
                    asset = GetAssetFromDevice<object>(assetData.Name, assetData.Is);
                    Assets[assetData.Name] = asset;
                    break;
            }
        }

        public Asset<T> CreateSensor<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name, "sensor");
            return asset;
        }

        public Asset<T> CreateActuator<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name, "actuator");
            return asset;
        }

        public Asset<T> CreateVirtual<T>(string name)
        {
            var asset = GetAssetFromDevice<T>(name, "virtual");
            return asset;
        }

        /********** -----Private methods----- **********/

        private Asset<T> GetAssetFromDevice<T>(string name, string kind)
        {
            if (Assets.ContainsKey(name))
            {
                var asset = Assets[name];
                if (asset.Attached)
                    return null;

                asset.SetDevice(this);
                asset.Attached = true;
                return asset as Asset<T>;
            }

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

            var newAsset = new Asset<T>(this, name, profile, kind);

            Assets[name] = newAsset;
            if(!AssetDatas.ContainsKey(name))
                Client.CreateAsset(this, newAsset);

            return newAsset;
        }
    }
}
