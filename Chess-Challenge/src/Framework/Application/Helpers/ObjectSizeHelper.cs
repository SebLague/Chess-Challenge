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

        public static long GetSize(object? obj) => GetSize(obj, new HashSet<object>());

        static long GetSize(object? obj, HashSet<object> seenObjects)
        {
            if (obj is null)
            {
                return 0;
            }

            var type = obj.GetType();

            // ignore references to the API board and the API Timer
            if (typeof(API.Board) == type || typeof(API.Timer) == type)
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
                // Marshal.SizeOf() also does not work with generics, so we need to use Unsafe.SizeOf()
                var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                return fieldInfos.Any(x => x.FieldType.IsClass) 
                    ? GetFieldsMemorySize(obj, fieldInfos, seenObjects) 
                    : GetStructSize(type);
            }
            
            if (type == typeof(string))
            {
                var str = (string)obj;
                return str.Length * 2 + 6 + ObjectSize;
            }

            if (obj is IEnumerable enumerable)
            {
                return ObjectSize + enumerable.Cast<object>().Sum(item => GetObjectSize(item, item?.GetType() ?? typeof(object), seenObjects));
            }
                
            if (type.IsClass)
            {
                return ObjectSize + GetFieldsMemorySize(obj, type, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, seenObjects);
            }

            throw new ArgumentException($"Unknown type {type.Name}", nameof(obj));
        }

        static readonly Dictionary<Type, int> sizeOfTypeCache = new();
        private static long GetStructSize(Type type)
        {
            if (!sizeOfTypeCache.TryGetValue(type, out var sizeOfType))
            {
                var genericSizeOf = typeof(System.Runtime.CompilerServices.Unsafe)
                    .GetMethod(nameof(System.Runtime.CompilerServices.Unsafe.SizeOf))
                    .MakeGenericMethod(type);

                sizeOfTypeCache[type] = sizeOfType = (int)genericSizeOf.Invoke(null, null)!;
            }

            return sizeOfType;
        }

        private static long GetFieldsMemorySize(object obj, Type type, BindingFlags bindingFlags, HashSet<object> seenObjects) 
            => GetFieldsMemorySize(obj, type.GetFields(bindingFlags), seenObjects);

        private static long GetFieldsMemorySize(object obj, IEnumerable<FieldInfo> fieldInfos, HashSet<object> seenObjects) 
            => fieldInfos.Sum(x => GetObjectSize(x.GetValue(obj), x.FieldType, seenObjects));
        
        private static long GetObjectSize(object? obj, Type type, HashSet<object> seenObjects)
        {
            if (type.IsClass && obj is not null && seenObjects.Contains(obj))
            {
                return 0;
            }
            
            var size = GetSize(obj, seenObjects) + (type.IsClass ? PointerSize : 0);

            if (type.IsClass && obj is not null)
            {
                seenObjects.Add(obj);
            }
            
            return size;
        }
    }
}
