/*
 * This file is part of the Console_Selenium_Serilog_Template Project.
 * 
 * This library is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this library. If not, see <https://www.gnu.org/licenses/>.
 * 
 * (c) 2020-2024 Braxton Bell
 * bell.braxton@outlook.com
 * https://www.linkedin.com/in/braxton-bell/
 */

using Console_Selenium_Serilog_Template.Utilities;

namespace Console_Selenium_Serilog_Template
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var startpp = new Startup();
            Task<IOperationResult> resultTask = startpp.StartAsync();

            IOperationResult result = resultTask.GetAwaiter().GetResult();

            if (result.Success)
            {
                Console.WriteLine("The application ran successfully.");
            }
            else
            {
                Console.WriteLine("The application failed to run.");
            }

        }
    }
}
