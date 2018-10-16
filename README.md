# C-Sharp-SDK
AllThingsTalk C# SDK provides C# APIs to manage and implement AllThingsTalk devices.

# Installation
Install NuGet from https://www.nuget.org/packages/AllThingsTalk/
or
Download SDK and open in Visual Studio. Build the project and include built AllThingsTalk.dll in your project.
Add dependencies.

# Dependencies
MQTTnet 2.8.2 https://github.com/chkr1011/MQTTnet

Newtonsoft.Json 11.0.2 https://www.newtonsoft.com/json

# Quickstart
Create an account in AllThingsTalk Maker, and create a device. All examples require DeviceID and DeviceToken, which can be found under Device Settings.

```C#
using AllThingsTalk;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSensor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new Client("<DeviceToken>");
            var counterDevice = await client.AttachDeviceAsync("<DeviceId>");
            var counter = await counterDevice.CreateSensorAsync<int>("Counter");

            for (var i = 0; i < 10; ++i)
            {
                await counter.PublishStateAsync(i);
                Thread.Sleep(2000);
            }

            Console.ReadLine();
        }
    }
}
```

# HowTo

### Initializing a client and a device
To manage a device, you must obtain a DeviceID (which is an unique device identifier within AllThingsTalk Maker) and DeviceToken (which this SDK uses to obtain access to APIs), both of which can be found on Device Settings page within AllThingsTalk Maker.

```C#
var client = new Client("<DeviceToken>");
var device = = await client.AttachDeviceAsync("<DeviceId>");
```

### Creating assets
Assets are added to initialized device and if they don't exist in maker platform, they are also created there. There are 3 types of assets: sensor, actuator and virtual.

#### Creating a sensor
Sensor is added by:
```C#
var sensor = await counterDevice.CreateSensorAsync<int>("Counter");
```
where `int` defines the data type and `sensorName` is the identifier that has to be the same as the one used in maker, if you want to attach to an existing sensor. Be sure to add Thread.Sleep(2000) so there is enough time for sensor to be created before we send any data.

#### Sending sensor data
Data is sent through:
```C#
await temperature.PublishStateAsync(23);
```
which will update `temperature` asset state in Maker with value `23`. If we send the wrong type an exception will be thrown.

#### Creating an actuator
This is a complete, simplest code for adding an actuator.
```C#
using AllThingsTalk;
using System;
using System.Threading.Tasks;

namespace ConsoleActuator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new Client("<DeviceToken>");
            var counterDevice = await client.AttachDeviceAsync("<DeviceId>");
            var button = await counterDevice.CreateActuatorAsync<bool>("Button");
            button.OnCommand += OnCommandHandler;
            Console.ReadLine();
        }

        private static void OnCommandHandler(object sender, Asset asset)
        {
            Console.WriteLine(asset.State.State.Value);
        }
    }
}
```
This code will create an asset as actuator (if it is not already there), with name "Button" and type boolean, under Device we used to initialize the code. In Maker, we create a pinboard with this asset and can send boolean value to the app, which will be displayed in the console.

#### Attach event handler to an actuator
```C#
button.OnCommand += OnCommandHandler;
```
Method you attach to an actuator will receive `Object sender`, which in this case is the `Device` that holds the `Asset` and `Asset` itself, from which you can retrieve `State`, which holds the `Value`, sent from `Maker`.
