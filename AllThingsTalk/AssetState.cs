using Newtonsoft.Json.Linq;
using System;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AllThingsTalk
{
    public class AssetState
    {
        // TODO: take this out. It is asset id.
        public string Id { get; set; }

        public State State { get; }

        // TODO: take this out
        internal AssetState(MqttMsgPublishEventArgs args)
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