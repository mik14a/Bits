/*
 * Copyright © 2020 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Text.Humanize
{
    /// <summary>
    /// Multiples of submultiple of the decimal.
    /// </summary>
    public enum Multiple
    {
        Decimal,
        Binary
    }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Binary_prefix
    /// </summary>
    public class BinaryPrefix
    {
        public static readonly BinaryPrefix[] Decimal;
        public static readonly BinaryPrefix[] Binary;
        public static readonly BinaryPrefix None;

        static BinaryPrefix() {
            Decimal = new BinaryPrefix[] {
                new BinaryPrefix("exa", "E", 1000000000000000000),
                new BinaryPrefix("peta", "P", 1000000000000000),
                new BinaryPrefix("tera", "T", 1000000000000),
                new BinaryPrefix("giga", "G", 1000000000),
                new BinaryPrefix("mega", "M", 1000000),
                new BinaryPrefix("kilo", "k", 1000),
            };
            Binary = new BinaryPrefix[] {
                new BinaryPrefix("exbi", "Ei", 1152921504606846976),
                new BinaryPrefix("pebi", "Pi", 1125899906842624),
                new BinaryPrefix("tebi", "Ti", 1099511627776),
                new BinaryPrefix("gibi", "Gi", 1073741824),
                new BinaryPrefix("mebi", "Mi", 1048576),
                new BinaryPrefix("kibi", "Ki", 1024),
            };
            None = new BinaryPrefix(string.Empty, string.Empty, 1);
        }

        /// <summary>Name of binary prefix.</summary>
        public string Name { get; }

        /// <summary>Symbol of binary prefix.</summary>
        public string Symbol { get; }

        /// <summary>Value of binary prefix.</summary>
        public long Integer { get; } = 1;

        /// <summary>Construct binary prefix. Only for use create table of submultiple.</summary>
        BinaryPrefix(string name, string symbol, long integer) {
            Symbol = symbol;
            Name = name;
            Integer = integer;
        }
    }

    /// <summary>
    /// Extend System.Int64.
    /// </summary>
    public static class Int64Extensions
    {
        public static string ToString(this long value, Multiple multiple) {
            var bp = _multiples[multiple].FirstOrDefault(p => 0 < value / p.Integer) ?? BinaryPrefix.None;
            var n = Math.Round((double)value / bp.Integer, 3);
            return string.Format("{0:0.000}{1}", n, bp.Symbol);
        }

        static readonly Dictionary<Multiple, BinaryPrefix[]> _multiples = new Dictionary<Multiple, BinaryPrefix[]> {
            { Multiple.Decimal, BinaryPrefix.Decimal },
            { Multiple.Binary, BinaryPrefix.Binary }
        };
    }

    /// <summary>
    /// Humanize number formatter.
    /// </summary>
    public class HumanizeNumberFormatter : IFormatProvider, ICustomFormatter
    {
        object IFormatProvider.GetFormat(Type formatType) {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider) {
            if (!Equals(formatProvider)) return null;
            var number = Convert.ToInt64(arg);
            switch (format ?? "B") {
            case "D": return number.ToString(Multiple.Decimal);
            case "B": return number.ToString(Multiple.Binary);
            default: return arg as string;
            }
        }
    }
}
