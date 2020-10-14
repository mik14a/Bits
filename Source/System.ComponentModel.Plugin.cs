/*
 * Copyright © 2020 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.ComponentModel.Plugin
{
    /// <summary>
    /// Plugin interface.
    /// </summary>
    public interface IPlugin : IDisposable { }

    /// <summary>
    /// Plugin host interface.
    /// </summary>
    public interface IPluginHost { }

    /// <summary>
    /// Plugin attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [ComVisible(true)]
    public class PluginAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; }

        public PluginAttribute(string name, string description) {
            Name = name;
            Description = description;
        }
    }

    /// <summary>
    /// Plugin info.
    /// </summary>
    /// <typeparam name="T">Plugin type.</typeparam>
    public class PluginInfo<T>
        where T : IPlugin
    {
        public Assembly Assembly { get; }

        public Type Type { get; }

        public string Name { get; }

        public string Description { get; }

        public PluginInfo(Assembly assembly, Type type) {
            Assembly = assembly;
            Type = type;
            var attribute = Type.GetCustomAttribute<PluginAttribute>();
            Name = attribute?.Name ?? Type.Name;
            Description = attribute?.Description;
        }

        public T CreateInstance() {
            var construct = Expression.Lambda<Func<T>>(Expression.New(Type)).Compile();
            return construct();
        }

        public static PluginInfo<T>[] LoadPlugins(string path) {
            return LoadPlugins(path, "*.dll");
        }

        public static PluginInfo<T>[] LoadPlugins(string path, string searchPattern) {
            return LoadPlugins(path, searchPattern, SearchOption.AllDirectories);
        }

        public static PluginInfo<T>[] LoadPlugins(string path, string searchPattern, SearchOption searchOption) {
            if (!Directory.Exists(path)) {
                return null;
            }

            var plugins = new List<PluginInfo<T>>();
#if NET35
            var pluginFiles = Directory.GetFiles(path, searchPattern, searchOption);
#else 

            var pluginFiles = Directory.EnumerateFiles(path, searchPattern, searchOption);
#endif
            foreach (var pluginFile in pluginFiles) {
                var assembly = Assembly.LoadFrom(pluginFile);
                foreach (var type in assembly.GetTypes()) {
                    var isInstanceable = type.IsClass && type.IsPublic && !type.IsAbstract;
                    var isAssignable = typeof(T).IsAssignableFrom(type);
                    if (isInstanceable && isAssignable) {
                        plugins.Add(new PluginInfo<T>(assembly, type));
                    }
                }
            }
            return plugins.ToArray();
        }
    }
}
