﻿/*
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

using AllThingsTalk;
using System;
using System.Threading;

namespace ConsoleSensor
{
    internal static class Program
    {
        private static MyLogger _logger;
        public static void Main(string[] args)
        {
            _logger = new MyLogger();
            var client = new Client("maker:4MPVlWZArchGW1VeVpnhn2PzyHu7dmLnGvPmcM5", _logger);
            var counterDevice = client.AttachDeviceAsync("Z8A5wkIq5XVM0dfMbZ1Jg4zH").Result;
            var counter = counterDevice.CreateSensor<int>("Counter");
            Thread.Sleep(2000);

            for (var i = 0; i < 10; ++i)
            {
                counter.PublishState(i);
                Thread.Sleep(2000);
            }

            Console.ReadLine();
        }
    }
}