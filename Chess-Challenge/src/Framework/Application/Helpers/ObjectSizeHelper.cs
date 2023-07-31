using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ChessChallenge.Application
{
    public static class ObjectSizeHelper
    {
        static readonly long ObjectSize = IntPtr.Size == 8 ? 24 : 12;
        static readonly long PointerSize = IntPtr.Size;

        public static long GetSize(object? obj)
        {
            if (obj is null)
            {
                return 0;
            }

            var type = obj.GetType();

            // ignore references to the API board
            if (typeof(API.Board) == type)
            {
                return 0;
            }

            if (type.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                return Marshal.SizeOf(underlyingType); 
            }
            
            if (type.IsValueType)
            {
                // Marshal.SizeOf() does not work for structs with reference types
                var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                return fieldInfos.Any(x => x.FieldType.IsClass) 
                    ? GetFieldsMemorySize(obj, fieldInfos) 
                    : Marshal.SizeOf(obj);
            }
            
            if (type == typeof(string))
            {
                var str = (string)obj;
                return str.Length * 2 + 6 + ObjectSize;
            }

            if (obj is IEnumerable enumerable)
            {
                return ObjectSize + enumerable.Cast<object>().Sum(item => GetObjectSize(item, item?.GetType() ?? typeof(object)));
            }
                
            if (type.IsClass)
            {
                return ObjectSize + GetFieldsMemorySize(obj, type, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            throw new ArgumentException($"Unknown type {type.Name}", nameof(obj));
        }

        private static long GetFieldsMemorySize(object obj, Type type, BindingFlags bindingFlags) 
            => GetFieldsMemorySize(obj, type.GetFields(bindingFlags));

        private static long GetFieldsMemorySize(object obj, IEnumerable<FieldInfo> fieldInfos) 
            => fieldInfos.Sum(x => GetObjectSize(x.GetValue(obj), x.GetType()));
        
        private static long GetObjectSize(object? obj, Type type) 
            => GetSize(obj) + (type.IsClass ? PointerSize : 0);
    }
}
