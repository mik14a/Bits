/*
 * Copyright © 2023 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [BenchmarkClass]
    class RandomBenchmark
    {
        [BenchmarkMethod]
        public void DefaultBenchmark() {
            var generator = new Random();
            _ = Enumerable.Repeat(generator, 100000).Select(r => r.Next()).ToArray();
        }
        [BenchmarkMethod]
        public void RNGCryptoBenchmark() {
            var generator = RandomNumberGenerator.Create("System.Security.Cryptography.RNGCryptoServiceProvider");
            var buffer = new byte[sizeof(int)];
            _ = Enumerable.Repeat(generator, 100000).Select(r => { r.GetBytes(buffer); return buffer; }).Select(b => BitConverter.ToInt32(b, 0)).ToArray();
        }
    }
}
