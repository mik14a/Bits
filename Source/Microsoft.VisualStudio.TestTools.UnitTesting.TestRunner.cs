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
#if NET35
    class TestClassAttribute : Attribute { }

    class TestInitializeAttribute : Attribute { }

    class TestCleanupAttribute : Attribute { }

    class TestMethodAttribute : Attribute { }

    class ExpectedExceptionAttribute : Attribute
    {
        public Type ExceptionType { get; }
    }
#endif

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
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), $"{nameof(assembly)} is null.");

            foreach (var type in assembly.GetTypes()) {
                if (type.GetCustomAttribute<TestClassAttribute>() != null) {
                    Run(type);
                }
            }
        }

        /// <summary>
        /// Run all tests.
        /// </summary>
        /// <param name="type">Target type.</param>
        public void Run(Type type) {
            if (type == null)
                throw new ArgumentNullException(nameof(type), $"{nameof(type)} is null.");

            if (type.GetCustomAttribute<TestClassAttribute>() == null) return;
            var construct = Expression.Lambda(Expression.New(type)).Compile();
            var instance = construct.DynamicInvoke();
            RunTests(instance);
        }

        /// <summary>
        /// Run all tests.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        public void Run<T>() {
            if (typeof(T).GetCustomAttribute<TestClassAttribute>() == null) return;
            var construct = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
            var instance = construct();
            RunTests(instance);
        }

        static void RunTests(object instance) {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), $"{nameof(instance)} is null.");

            Trace.WriteLine($"Class [{instance.GetType()}]");
            var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var initializes = methods.Where(methodInfo => methodInfo.GetCustomAttribute<TestInitializeAttribute>() != null).ToArray();
            var cleanups = methods.Where(methodInfo => methodInfo.GetCustomAttribute<TestCleanupAttribute>() != null).ToArray();
            methods = methods.Where(methodInfo => methodInfo.GetCustomAttribute<TestMethodAttribute>() != null).ToArray();
            if (initializes.All(methodInfo => RunTestInitialize(instance, methodInfo))) {
                methods.All(methodInfo => RunTestMethod(instance, methodInfo));
                cleanups.All(methodInfo => RunTestCleanup(instance, methodInfo));
            }
        }

        static bool RunTestInitialize(object instance, MethodInfo methodInfo) {
            Trace.WriteLine($"  TestInitialize [{methodInfo}]");
            return RunTestMethodInternal(instance, methodInfo);
        }

        static bool RunTestMethod(object instance, MethodInfo methodInfo) {
            Trace.WriteLine($"  TestMethod [{methodInfo}]");
            return RunTestMethodInternal(instance, methodInfo);
        }

        static bool RunTestCleanup(object instance, MethodInfo methodInfo) {
            Trace.WriteLine($"  TestCleanup [{methodInfo}]");
            return RunTestMethodInternal(instance, methodInfo);
        }

        static bool RunTestMethodInternal(object instance, MethodInfo methodInfo) {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), $"{nameof(instance)} is null.");
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo), $"{nameof(methodInfo)} is null.");

            var expectedExceptions = methodInfo.GetCustomAttributes<ExpectedExceptionAttribute>().ToArray();
            try {
                var methodCall = Expression.Call(Expression.Constant(instance), methodInfo);
                var testMethod = Expression.Lambda<Action>(methodCall).Compile();
                testMethod();
                return true;
            } catch (Exception ex) when (expectedExceptions.Any(e => e.ExceptionType == ex.GetType())) {
                return false;
            }
        }
    }
}
