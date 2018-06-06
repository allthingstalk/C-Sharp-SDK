# C-Sharp-SDK
C# client for connecting to AllThingsTalk platform.

AllThingsTalk C# SDK provides C# APIs to manage and implement AllThingsTalk devices.

# Installation

# Quickstart
You should create an AllThingsTalk Maker account, and a simple device. All examples require DeviceID and DeviceToken, which can be found under Device Settings.

# Dependencies
M2MqttDotnetCore 1.0.7 https://github.com/mohaqeq/paho.mqtt.m2mqtt

Newtonsoft.Json 11.0.2 https://www.newtonsoft.com/json

```C#
using AllThingsTalk;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            logger = new MyLogger();
            var client = new Client("<DeviceToken>");
            var counterDevice = client.AttachDevice("<DeviceId>");
            var counter = counterDevice.CreateSensor<int>("Counter");
            
            for (var i = 0; i < 10; ++i)
            {
                counter.PublishState(i);
                Thread.Sleep(2000);
            }
        }
    }
}
```
