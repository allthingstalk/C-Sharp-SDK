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

using AllThingsTalk;
//using NLog;
using System;

namespace ConsoleApp1
{
    public class MyLogger: ILogger
    {
        //static Logger _logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Writes a diagnostic message at the trace level to the desired output using the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args">any arguments to replace in the message.</param>
        public void Trace(string message, params object[] args)
        {
            //_logger.Trace(message, args);
            Console.WriteLine("trace: " + message, args);
        }

        /// <summary>
        /// Writes a diagnostic message at the infor level to the desired output using the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args">any arguments to replace in the message.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Info(string message, params object[] args)
        {
            //_logger.Info(message, args);
            Console.WriteLine("Info: " + message, args);
        }

        /// <summary>
        /// Writes a diagnostic message at the warning level to the desired output using the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args">any arguments to replace in the message.</param>
        public void Warn(string message, params object[] args)
        {
            //_logger.Warn(message, args);
            Console.WriteLine("Warn: " + message, args);
        }

        /// <summary>
        /// Writes a diagnostic message at the error level to the desired output using the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args">any arguments to replace in the message.</param>
        public void Error(string message, params object[] args)
        {
            //_logger.Error(message, args);
            Console.WriteLine("Error: " + message, args);
        }
    }
}
