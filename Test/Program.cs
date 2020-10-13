/*
 * Copyright © 2017 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
#if NET35 || NET452 || NET462 || NET472 || NET48
    class Program
    {
        static void Main(string[] args) {
            Console.WriteLine($"Running [{Assembly.GetEntryAssembly().GetName()}]");
            var program = new TestRunner();
            program.Run(Assembly.GetEntryAssembly());
        }
    }
#endif

}
