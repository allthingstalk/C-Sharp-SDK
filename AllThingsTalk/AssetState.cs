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
using Newtonsoft.Json.Linq;

namespace AllThingsTalk
{
    public class AssetState
    {
        public State State { get; }

        internal AssetState(string val)
        {
            JToken value;

            try
            {
                value = JToken.Parse(val);
            }
            catch
            {
                ////compensate for strings: they are sent without ""
                ////for arduino, low bandwith, but strict json requires quotes
                val = "\"" + val + "\"";
                value = JToken.Parse(val);
            }

            State = value.ToObject<State>();
        }

        internal AssetState(JToken value)
        {
            State = new State
            {
                Value = value,
                At = DateTime.Now
            };
        }

        internal AssetState()
        {
        }
    }

    public class State
    {
        public JToken Value { get; set; }
        public DateTime At { get; set; }
    }
}