using Newtonsoft.Json.Linq;
using System;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AllThingsTalk
{
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
                //compensate for strings: they are sent without ""
                //for arduino, low bandwith, but strict json requires quotes
                val = "\"" + val + "\"";
                value = JToken.Parse(val);
            }

            State = value.ToObject<State>();
        }
    }
}