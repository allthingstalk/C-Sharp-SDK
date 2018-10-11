using AllThingsTalk;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleCoreSensor
{
    internal static class Program
    {
        private static MyLogger _logger;
        static async Task Main(string[] args)
        {
            _logger = new MyLogger();
            var client = new Client("<DeviceToken>", _logger);
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
