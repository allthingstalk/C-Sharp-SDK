using AllThingsTalk;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

namespace DemoApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private const int LedPin = 12;
        private GpioPin _pin;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            //_logger = new Logger();
            InitGpio();

            var client = new Client("maker:4MPVlWZArchGW1VeVpnhn2PzyHu7dmLnGvPmcM5");
            var counterDevice = client.AttachDevice("Z8A5wkIq5XVM0dfMbZ1Jg4zH");
            var actuator = counterDevice.CreateActuator<bool>("Led");
            actuator.OnCommand += OnDeviceCommand;

        }

        public void OnDeviceCommand(object obj, AssetState asset)
        {
            switch (asset.Id)
            {
                case "Sensor2":
                    break;
                case "Color1":
                    break;
                default:
                    break;
            }

            Debug.WriteLine("Value is " + asset.State.Value);
            Debug.WriteLine("Value is " + asset.State.At);
            Debug.WriteLine("Value is " + asset.Id);
            Debug.WriteLine("Value is " + asset.ToString());
            Debug.WriteLine("Value is " + asset.State.Value.Type);
            Debug.WriteLine("end");

            if ((bool)asset.State.Value)
            {
                Debug.WriteLine("High");
                _pin.Write(GpioPinValue.High);
            }
            else
            {
                Debug.WriteLine("Low");
                _pin.Write(GpioPinValue.Low);
            }
        }

        private void InitGpio()
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                _pin = null;
                Debug.WriteLine("There is no GPIO controller on this device.");
                return;
            }

            _pin = gpio.OpenPin(LedPin);

            if (_pin == null)
            {
                Debug.WriteLine("Pin error.");
                return;
            }

            _pin.Write(GpioPinValue.Low);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
        }
    }
}
