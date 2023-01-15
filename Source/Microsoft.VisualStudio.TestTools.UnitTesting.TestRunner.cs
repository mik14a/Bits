/*
 * Copyright © 2017 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    /// <summary>
    /// The TestRunner.
    /// </summary>
    public class TestRunner
    {
        /// <summary>
        /// Run all tests.
        /// </summary>
        /// <param name="assembly">Target assembly.</param>
        public void Run(Assembly assembly) {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));

            var types = assembly.GetTypes().Where(type => type.GetCustomAttribute<TestClassAttribute>() != null).ToArray();
            WriteLine(ConsoleColor.White, $"TestRunner: Assembly [ {assembly} ], {types.Length} Classes.");
            var elapsed = TimeSpan.Zero;
            foreach (var type in types) {
                elapsed += Run(type);
            }
            WriteLine(ConsoleColor.White, $"TestRunner Done. Took {ToHumanize(elapsed)}.");
        }

        /// <summary>
        /// Run all tests.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        public TimeSpan Run<T>() {
            if (typeof(T).GetCustomAttribute<TestClassAttribute>() is null) return TimeSpan.Zero;
            return Run(typeof(T));
        }

        /// <summary>
        /// Run all tests.
        /// </summary>
        /// <param name="type">Target type.</param>
        public TimeSpan Run(Type type) {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (type.GetCustomAttribute<TestClassAttribute>() is null) return TimeSpan.Zero;
            return RunTests(type);
        }

        static TimeSpan RunTests(Type type) {
            var testClassRunner = new TestClassRunner(type);
            return testClassRunner.Run();
        }

        class TestClassRunner
        {
            public TestClassRunner(Type type) {
                var construct = Expression.Lambda(Expression.New(type)).Compile();
                _instance = construct.DynamicInvoke();
                var methods = _instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).ToArray();
                _initializes = methods.Where(IsTestInitialize).ToArray();
                _cleanups = methods.Where(IsTestCleanup).ToArray();
                _tests = methods.Where(IsTestMethod).ToArray();
                static bool IsTestInitialize(MethodInfo methodInfo) => methodInfo.GetCustomAttribute<TestInitializeAttribute>() != null;
                static bool IsTestCleanup(MethodInfo methodInfo) => methodInfo.GetCustomAttribute<TestCleanupAttribute>() != null;
                static bool IsTestMethod(MethodInfo methodInfo) => methodInfo.GetCustomAttribute<TestMethodAttribute>() != null;
            }

            public TimeSpan Run() {
                var count = _initializes.Length + _tests.Length + _cleanups.Length;
                WriteLine(ConsoleColor.White, $"\tTestClassRunner: Class [ {_instance.GetType()} ], {count} Methods.");
                var result = TimeSpan.Zero;
                if (_initializes.Length != 0) {
                    TimeSpan Run(MethodInfo initialize) => RunTestInitialize(_instance, initialize);
                    WriteLine(ConsoleColor.Green, $"\t\tRunning {_initializes.Length} TestInitializes.");
                    var elapsed = _initializes.Select(Run).Aggregate(TimeSpan.Zero, (t, e) => t + e);
                    WriteLine(ConsoleColor.Green, $"\tTestClassRunner Done. Took {ToHumanize(elapsed)}");
                    result += elapsed;
                }
                if (_tests.Length != 0) {
                    TimeSpan Run(MethodInfo test) => RunTestMethod(_instance, test);
                    WriteLine(ConsoleColor.Green, $"\t\tRunning {_tests.Length} Tests.");
                    var elapsed = _tests.Select(Run).Aggregate(TimeSpan.Zero, (t, e) => t + e);
                    WriteLine(ConsoleColor.Green, $"\t\tDone. Took {ToHumanize(elapsed)}");
                    result += elapsed;
                }
                if (_cleanups.Length != 0) {
                    TimeSpan Run(MethodInfo cleanup) => RunTestCleanup(_instance, cleanup);
                    WriteLine(ConsoleColor.Green, $"\t\tRunning {_cleanups.Length} Cleanups.");
                    var elapsed = _cleanups.Select(Run).Aggregate(TimeSpan.Zero, (t, e) => t + e);
                    WriteLine(ConsoleColor.Green, $"\t\tDone. Took {ToHumanize(elapsed)}");
                    result += elapsed;
                }
                WriteLine(ConsoleColor.White, $"\tDone. Took {ToHumanize(result)}.");
                return result;
            }

            static TimeSpan RunTestInitialize(object instance, MethodInfo testInitialize) {
                WriteLine(ConsoleColor.Cyan, $"\t\t\tInitialize [ {testInitialize} ].");
                var initialize = CompileMethod(instance, testInitialize);
                var stopwatch = Stopwatch.StartNew();
                initialize();
                stopwatch.Stop();
                WriteLine(ConsoleColor.Cyan, $"\t\t\tDone. Took {ToHumanize(stopwatch.Elapsed)}.");
                return stopwatch.Elapsed;
            }

            static TimeSpan RunTestCleanup(object instance, MethodInfo testCleanup) {
                WriteLine(ConsoleColor.Cyan, $"\t\t\tRunTestCleanup [ {testCleanup} ].");
                var cleanup = CompileMethod(instance, testCleanup);
                var stopwatch = Stopwatch.StartNew();
                cleanup();
                stopwatch.Stop();
                WriteLine(ConsoleColor.Cyan, $"\t\t\tDone. Took {ToHumanize(stopwatch.Elapsed)}.");
                return stopwatch.Elapsed;
            }

            static TimeSpan RunTestMethod(object instance, MethodInfo testMethod) {
                var expectedExceptions = testMethod.GetCustomAttributes<ExpectedExceptionAttribute>().ToArray();
                WriteLine(ConsoleColor.Cyan, $"\t\t\tTest [ {testMethod} ].");
                var test = CompileMethod(instance, testMethod);
                var stopwatch = Stopwatch.StartNew();
                try {
                    test();
                } catch (Exception ex) when (expectedExceptions.Any(e => e.ExceptionType == ex.GetType())) {
                    // Catch expected exceptions.
                }
                stopwatch.Stop();
                WriteLine(ConsoleColor.Cyan, $"\t\t\tDone. Took {ToHumanize(stopwatch.Elapsed)}.");
                return stopwatch.Elapsed;
            }

            static Action CompileMethod(object instance, MethodInfo methodInfo) {
                var methodCall = Expression.Call(Expression.Constant(instance), methodInfo);
                return Expression.Lambda<Action>(methodCall).Compile();
            }

            readonly object _instance;
            readonly MethodInfo[] _initializes;
            readonly MethodInfo[] _cleanups;
            readonly MethodInfo[] _tests;
        }

        static void WriteLine(ConsoleColor color, string value) {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = previous;
        }

        static string ToHumanize(TimeSpan timeSpan)
           => timeSpan.Milliseconds < 1 ? string.Format("< 1 Milliseconds")
           : timeSpan.Seconds < 1 ? string.Format("{0:0.00} Milliseconds", (float)timeSpan.Ticks / TimeSpan.TicksPerMillisecond)
           : timeSpan.Minutes < 60 ? string.Format("{0:0.00} Seconds", (float)timeSpan.Ticks / TimeSpan.TicksPerSecond)
           : timeSpan.Hours < 24 ? string.Format("{0:0.00} Minutes", (float)timeSpan.Ticks / TimeSpan.TicksPerMinute)
           : timeSpan.Days < 1 ? string.Format("{0:0.00} Hours", (float)timeSpan.Ticks / TimeSpan.TicksPerHour)
           : string.Format("{0:0.00} Days", (float)timeSpan.Ticks / TimeSpan.TicksPerDay);
    }
}
