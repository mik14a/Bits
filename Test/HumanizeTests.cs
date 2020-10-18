using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Text.Humanize.Tests
{
    [TestClass()]
    public class HumanizeTests
    {
        class TestCase
        {
            public long Integer { get; }

            public Dictionary<Multiple, string> Value { get; }

            public TestCase(long integer, Dictionary<Multiple, string> value) {
                Integer = integer;
                Value = value;
            }
        }

        static TestCase[] _cases = new TestCase[] {
            new TestCase(1000, new Dictionary<Multiple, string>() { 
                { Multiple.Decimal, "1.000k" }, { Multiple.Binary, "1000.000" }
            }),
            new TestCase(1024, new Dictionary<Multiple, string>() {
                { Multiple.Decimal, "1.024k" }, { Multiple. Binary, "1.000Ki" }
            })
        };

        [TestMethod()]
        public void ToStringTest() {
            foreach (var @case in _cases) {
                var decimalString = @case.Integer.ToString(Multiple.Decimal);
                Assert.AreEqual(@case.Value[Multiple.Decimal], decimalString);
                var binaryString = @case.Integer.ToString(Multiple.Binary);
                Assert.AreEqual(@case.Value[Multiple.Binary], binaryString);
            }
        }

        [TestMethod()]
        public void FormatTest() {
            var formatter = new HumanizeNumberFormatter();
            foreach (var @case in _cases) {
                var decimalString = string.Format(formatter, "{0:D}", @case.Integer);
                Assert.AreEqual(@case.Value[Multiple.Decimal], decimalString);
                var binaryString = string.Format(formatter, "{0:B}", @case.Integer);
                Assert.AreEqual(@case.Value[Multiple.Binary], binaryString);
                // Test default format.
                var defaultString = string.Format(formatter, "{0}", @case.Integer);
                Assert.AreEqual(@case.Value[Multiple.Binary], defaultString);
            }
        }
    }
}
