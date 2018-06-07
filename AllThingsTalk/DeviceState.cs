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
    using Newtonsoft.Json.Linq;
    using uPLibrary.Networking.M2Mqtt.Messages;

    public class DeviceState
    {
        public string Id { get; }
        public State State { get; }

        public DeviceState(MqttMsgPublishEventArgs args)
        {
            var parts = args.Topic.Split(new char[] { '/' });
            Id = parts[3];

            var val = System.Text.Encoding.UTF8.GetString(args.Message);
            JToken value;

            try
            {
                value = JToken.Parse(val);
            }
            catch
            {
                val = "\"" + val + "\"";
                value = JToken.Parse(val);
            }

            State = value.ToObject<State>();
        }
    }
}