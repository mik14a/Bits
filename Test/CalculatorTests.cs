/*
 * Copyright © 2017 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Scripting.Tests
{
    [TestClass]
    public class CalculatorTests
    {
        [TestMethod]
        public void Usage() {
            Console.WriteLine(Calculator.Calc("1 + 2 + 3 + 4"));
        }

        [TestMethod]
        public void CalcTest() {
            var expressions = new Dictionary<string, int>() {
                { string.Empty, 0 },
                { "1", 1 },
                { "1 + 2 + 3 + 4", 10 },
                { "1 * 2 + 3 * 4", 14 },
                { "1 + 2 * 3 + 4", 11 },
                { "1 - 2 + 3 - 4", -2 },
                { "1 / 2 * 3 / 4", 0 }
            };
            foreach (var expression in expressions) {
                var key = expression.Key;
                var value = expression.Value;
                var rpn = string.Join(" ", Calculator.Rpn.Parse(key).Select(t => t.Value));
                var result = Calculator.Calc(key);
                Console.WriteLine($"Expression '{key}', Value '{value}', Rpn '{rpn}', Result {result}'");
                Assert.AreEqual(value, result);
            }

        }
    }
}
