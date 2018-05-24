using AllThingsTalk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace DemoApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private const int LED_PIN = 12;
        private GpioPin pin;
        //private Logger _logger;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            //_logger = new Logger();
            InitGpio();
            Debug.WriteLine("Inside");
            var client = new Client("maker:4MPVlWZArchGW1VeVpnhn2PzyHu7dmLnGvPmcM5");
            client.AttachDevice("Z8A5wkIq5XVM0dfMbZ1Jg4zH");
            client.OnAssetStateEvent += OnActuatorValue;
        }

        private void OnActuatorValue(object sender, AssetState e)
        {
            //Debug.WriteLine("incomming value found: {0}", e.ToString());

            Debug.WriteLine("Value is " + e.State.Value);
            Debug.WriteLine("Value is " + e.State.At);
            Debug.WriteLine("Value is " + e.Id);
            Debug.WriteLine("Value is " + e.ToString());
            Debug.WriteLine("Value is " + e.State.Value.Type);
            Debug.WriteLine("end");
            //check the actuator for which we received a command
            //the actuator id always comes in as a string.

            if ((bool)e.State.Value)
            {
                Debug.WriteLine("High");
                pin.Write(GpioPinValue.High);
            }
            else
            {
                Debug.WriteLine("Low");
                pin.Write(GpioPinValue.Low);
            }
        }

        private void InitGpio()
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                pin = null;
                Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }

            pin = gpio.OpenPin(LED_PIN);

            if (pin == null)
            {
                Debug.WriteLine("Pin error.");
                return;
            }

            //pin = GpioController.GetDefault().OpenPin(LED_PIN);
            pin.Write(GpioPinValue.Low);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }
    }
}
