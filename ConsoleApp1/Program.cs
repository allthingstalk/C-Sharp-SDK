/*
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


namespace ConsoleApp1
{
    using AllThingsTalk;
    using System.Threading;

    internal class Program
    {
        private static MyLogger _logger;
        public static void Main(string[] args)
        {
            _logger = new MyLogger();
            var client = new Client("spicy:4HcnDAW3CvqaW1VeVpesT2mAKdgere1pfbTryUM", _logger, "https://spicy.allthingstalk.io", "spicy.allthingstalk.io");
            var counterDevice = client.AttachDeviceAsync("KAhsmCXNbQc1RLFQgvDCXjIX");
            var counter = counterDevice.CreateSensor<int>("Counter");

            for (var i = 0; i < 10; ++i)
            {
                counter.PublishState(i);
                Thread.Sleep(2000);
            }
        }
    }
}
