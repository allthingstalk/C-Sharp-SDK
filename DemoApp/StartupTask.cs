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
using System.Diagnostics;
using AllThingsTalk;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

namespace DemoApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        //depends on your physical device
        private const int LedPin = 12;
        private GpioPin _pin;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            InitGpio();

            var client = new Client("maker:4MPVlWZArchGW1VeVpnhn2PzyHu7dmLnGvPmcM5");
            var counterDevice = client.AttachDeviceAsync("Z8A5wkIq5XVM0dfMbZ1Jg4zH");
            var actuator = counterDevice.CreateActuator<bool>("Led");
            actuator.OnCommand += OnDeviceCommand;
        }

        private void OnDeviceCommand(object sender, Asset asset)
        {
            if ((bool)asset.State.State.Value)
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
