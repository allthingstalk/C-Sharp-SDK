using System;
using AllThingsTalk;
using System.Threading;

namespace ConsoleCoreSensor
{
    internal static class Program
    {
        private static MyLogger _logger;
        static void Main(string[] args)
        {
            _logger = new MyLogger();
            var client = new Client("maker:4MPVlWZArchGW1VeVpnhn2PzyHu7dmLnGvPmcM5", _logger);
            var counterDevice = client.AttachDevice("Z8A5wkIq5XVM0dfMbZ1Jg4zH");
            var counter = counterDevice.CreateSensor<int>("Counter");

            for (var i = 0; i < 10; ++i)
            {
                counter.PublishState(i);
                Thread.Sleep(2000);
            }

            Console.ReadLine();
        }
    }
}
