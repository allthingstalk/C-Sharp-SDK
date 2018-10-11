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
using System.Threading.Tasks;

namespace AllThingsTalk
{
    public class Device
    {
        public string Id { get; }
        internal readonly Client Client;
        internal Dictionary<string, Asset> Assets { get; }
        internal Dictionary<string, AssetData> AssetDatas { get; }

        internal Device(Client client, string deviceId)
        {
            Client = client;
            Id = deviceId;
            Assets = new Dictionary<string, Asset>();
            AssetDatas = new Dictionary<string, AssetData>();
        }

        /********** -----Public methods----- **********/
        /// <summary>
        /// Create a sensor with specific type. If it isn't already defined in `maker` it will also be created there.
        /// Type can be: int, double, string, bool, object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">Sensor name</param>
        /// <returns>Sensor</returns>
        public async Task<Asset<T>> CreateSensorAsync<T>(string name)
        {
            var asset = await GetAssetFromDeviceAsync<T>(name, "sensor").ConfigureAwait(false);
            return asset;
        }

        /// <summary>
        /// Create an actuator with specific type. If it isn't already defined in `maker` it will also be created there.
        /// Type can be: int, double, string, bool, object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">Actuator name</param>
        /// <returns>Actuator</returns>
        public async Task<Asset<T>> CreateActuatorAsync<T>(string name)
        {
            var asset = await GetAssetFromDeviceAsync<T>(name, "actuator").ConfigureAwait(false);
            return asset;
        }

        /// <summary>
        /// Create a virtual asset with specific type. If it isn't already defined in `maker` it will also be created there.
        /// Type can be: int, double, string, bool, object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">Virtual asset name</param>
        /// <returns>Virtual asset</returns>
        public async Task<Asset<T>> CreateVirtualAsync<T>(string name)
        {
            var asset = await GetAssetFromDeviceAsync<T>(name, "virtual").ConfigureAwait(false);
            return asset;
        }

        /********** -----Internal methods----- **********/
        internal async Task CreateFromAssetDataAsync(AssetData assetData)
        {
            Asset asset;
            switch (assetData.Profile.Type)
            {
                case "integer":
                    asset = await GetAssetFromDeviceAsync<int>(assetData.Name, assetData.Is).ConfigureAwait(false);
                    Assets[assetData.Name] = asset;
                    break;
                case "number":
                    asset = await GetAssetFromDeviceAsync<double>(assetData.Name, assetData.Is).ConfigureAwait(false);
                    Assets[assetData.Name] = asset;
                    break;
                case "boolean":
                    asset = await GetAssetFromDeviceAsync<bool>(assetData.Name, assetData.Is).ConfigureAwait(false);
                    Assets[assetData.Name] = asset;
                    break;
                case "string":
                    asset = await GetAssetFromDeviceAsync<string>(assetData.Name, assetData.Is).ConfigureAwait(false);
                    Assets[assetData.Name] = asset;
                    break;
                case "object":
                    asset = await GetAssetFromDeviceAsync<object>(assetData.Name, assetData.Is).ConfigureAwait(false);
                    Assets[assetData.Name] = asset;
                    break;
            }
        }

        /********** -----Private methods----- **********/
        private async Task<Asset<T>> GetAssetFromDeviceAsync<T>(string name, string kind)
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
            if (!AssetDatas.ContainsKey(name))
                await Client.CreateAssetAsync(this, newAsset).ConfigureAwait(false);

            return newAsset;
        }
    }
}
