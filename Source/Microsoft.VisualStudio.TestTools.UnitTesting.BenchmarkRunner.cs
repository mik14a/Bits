/*
 * Copyright © 2023 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BenchmarkClassAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BenchmarkMethodAttribute : Attribute { }

    /// <summary>
    /// The BenchmarkRunner.
    /// </summary>
    public class BenchmarkRunner
    {
        /// <summary>
        /// Run all benchmarks.
        /// </summary>
        /// <param name="assembly">Target assembly.</param>
        public void Run(Assembly assembly) {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));

            Progress(ConsoleColor.White, $"Start benchmark assembly [ {assembly.GetName()} ].");
            foreach (var type in assembly.GetTypes()) {
                if (type.GetCustomAttribute<BenchmarkClassAttribute>() != null) {
                    Run(type);
                }
            }
            Progress(ConsoleColor.White, $"End benchmark.");
        }

        /// <summary>
        /// Run all benchmarks.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        public void Run<T>() => Run(typeof(T));

        /// <summary>
        /// Run all benchmarks.
        /// </summary>
        /// <param name="type">Target type.</param>
        public void Run(Type type) {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (type.GetCustomAttribute<BenchmarkClassAttribute>() is null) return;

            var construct = Expression.Lambda(Expression.New(type)).Compile();
            var stopwatch = Stopwatch.StartNew();
            var instance = construct.DynamicInvoke();
            stopwatch.Stop();
            Progress(ConsoleColor.Green, $"  Start benchmark class [ {type.Name} ]: Instancing {ToHumanize(stopwatch.Elapsed)}.");
            var time = RunBenchmarks(instance);
            Progress(ConsoleColor.Green, $"  End benchmark: Total {ToHumanize(time)}.");
        }

        static TimeSpan RunBenchmarks(object instance) {
            if (instance is null) throw new ArgumentNullException(nameof(instance));

            var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            methods = methods.Where(methodInfo => methodInfo.GetCustomAttribute<BenchmarkMethodAttribute>() != null).ToArray();
            var total = methods.Select(methodInfo => new MethodRunner(instance, methodInfo, 100))
                .Select(runner => runner.Benchmark())
                .Aggregate(TimeSpan.Zero, (t, s) => t + s);
            return total;
        }

        class MethodRunner
        {
            public MethodRunner(object instance, MethodInfo methodInfo, int run) {
                if (instance is null) throw new ArgumentNullException(nameof(instance));
                if (methodInfo is null) throw new ArgumentNullException(nameof(methodInfo));

                _methodName = methodInfo.Name;
                var methodCall = Expression.Call(Expression.Constant(instance), methodInfo);
                _benchmark = Expression.Lambda<Action>(methodCall).Compile();
                _run = run;
            }

            public TimeSpan Benchmark() {
                Progress(ConsoleColor.Cyan, $"    Start benchmark [ {_methodName} ] for {_run} times.");

                TimeSpan[] result;
                using (new ConsoleDisabler()) {
                    result = Enumerable.Repeat(_benchmark, _run).Select(benchmark => {
                        var stopwatch = Stopwatch.StartNew();
                        benchmark();
                        stopwatch.Stop();
                        return stopwatch.Elapsed;
                    }).ToArray();
                };

                var total = result.Aggregate(TimeSpan.Zero, (t, e) => t + e);
                var min = result.Min();
                var max = result.Max();
                var average = new TimeSpan(total.Ticks / _run);
                Progress(ConsoleColor.Cyan, string.Format(
                    "    End benchmark [ {0} ]: Total: {1}, Average: {2}, Min: {3}, Max: {4}.",
                    _methodName, ToHumanize(total), ToHumanize(average), ToHumanize(min), ToHumanize(max)));
                return total;
            }

            class ConsoleDisabler : IDisposable
            {
                public ConsoleDisabler() {
                    _output = Console.Out;
                    Console.SetOut(TextWriter.Null);
                }
                public void Dispose() => Console.SetOut(_output);
                readonly TextWriter _output;
            }

            readonly string _methodName;
            readonly Action _benchmark;
            readonly int _run;
        }

        static void Progress(ConsoleColor color, string message) {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }

        static string ToHumanize(TimeSpan timeSpan)
           => timeSpan.Milliseconds < 1 ? string.Format("{0} Ticks", timeSpan.Ticks)
           : timeSpan.Seconds < 1 ? string.Format("{0:0.00} Milliseconds", (float)timeSpan.Ticks / TimeSpan.TicksPerMillisecond)
           : timeSpan.Minutes < 60 ? string.Format("{0:0.00} Seconds", (float)timeSpan.Ticks / TimeSpan.TicksPerSecond)
           : timeSpan.Hours < 24 ? string.Format("{0:0.00} Minutes", (float)timeSpan.Ticks / TimeSpan.TicksPerMinute)
           : timeSpan.Days < 1 ? string.Format("{0:0.00} Hours", (float)timeSpan.Ticks / TimeSpan.TicksPerHour)
           : string.Format("{0:0.00} Days", (float)timeSpan.Ticks / TimeSpan.TicksPerDay);
    }
}
