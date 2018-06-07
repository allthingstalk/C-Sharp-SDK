# C-Sharp-SDK
C# client for connecting to AllThingsTalk platform.

AllThingsTalk C# SDK provides C# APIs to manage and implement AllThingsTalk devices.

# Installation

1) Using NuGet package manager add AllThingsTalk client to your project (to be added)
2) Download SDK and open in Visual Studio. Build the project and include built AllThingsTalk.dll in your project.
Add dependencies.

# Dependencies
M2MqttDotnetCore 1.0.7 https://github.com/mohaqeq/paho.mqtt.m2mqtt

Newtonsoft.Json 11.0.2 https://www.newtonsoft.com/json

# Quickstart
You should create an AllThingsTalk Maker account, and a simple device. All examples require DeviceID and DeviceToken, which can be found under Device Settings.

```C#
using AllThingsTalk;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
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

# HowTo

### Initializing a client and a device
To manage a device, you must obtain a DeviceID (which is an unique device identifier within AllThingsTalk Maker) and DeviceToken (which this SDK uses to obtain access to APIs), both of which can be found on Device Settings page within AllThingsTalk Maker.

```C#
var client = new Client("<DeviceToken>");
var device = client.AttachDevice("<DeviceId>");
```

### Creating assets
Assets are added to initialized device and if they don't exist in maker platform, they are also created there. There are 3 types of assets: sensor, actuator and virtual.

#### Creating a sensor
Sensor is added by:
```C#
var sensor = device.CreateSensor<int>("<sensorName>");
```
where `int` defines the data type and `sensorName` is the identifier that has to be the same as the one used in maker, if you want to attach to an existing sensor.
