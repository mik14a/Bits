/*
 * Copyright © 2020 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
#if NET35
    public static class CustomAttributeExtensions
    {
        public static T GetCustomAttribute<T>(this Type element)
            where T : Attribute {
            return element.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }
        public static T GetCustomAttribute<T>(this MemberInfo element)
            where T : Attribute {
            return element.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }
        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element)
            where T : Attribute {
            return element.GetCustomAttributes(typeof(T), true).Cast<T>();
        }
    }
#endif
}
