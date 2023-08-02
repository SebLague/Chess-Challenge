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
                if (!sizeOfTypeCache.TryGetValue(type, out var sizeOfType))
                {
                    var underlyingType = Enum.GetUnderlyingType(type);
                    sizeOfTypeCache[type] = sizeOfType = Marshal.SizeOf(underlyingType);
                }
                
                return sizeOfType; 
            }
            
            if (type.IsValueType)
            {
                // Marshal.SizeOf() does not work for structs with reference types
                // Marshal.SizeOf() also does not work with generics, so we need to use Unsafe.SizeOf()
                if (!fieldInfoCache.TryGetValue(type, out var fieldInfos))
                {
                    fieldInfoCache[type] = fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }
                if (!typeContainsClassCache.TryGetValue(type, out var typeContainsClass))
                {
                    typeContainsClassCache[type] = typeContainsClass = fieldInfos.Any(x => TypeIsClass(x.FieldType));
                }
                return typeContainsClass 
                    ? GetFieldsMemorySize(obj, fieldInfos, seenObjects) 
                    : GetStructSize(type);
            }
            
            if (type == typeof(string))
            {
                var str = (string)obj;
                return str.Length * 2 + 6 + ObjectSize;
            }
            
            if (obj is IList collection)
            {
                var totalSize = ObjectSize;
                
                for (var index = 0; index < collection.Count; index++)
                {
                    var item = collection[index];
                    totalSize += GetObjectSize(item, item?.GetType() ?? typeof(object), seenObjects);
                }

                return totalSize;
            }
                
            if (TypeIsClass(type))
            {
                if (!fieldInfoCache.TryGetValue(type, out var fieldInfos))
                {
                    fieldInfoCache[type] = fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }
                
                return ObjectSize + GetFieldsMemorySize(obj, fieldInfos, seenObjects);
            }

            throw new ArgumentException($"Unknown type {type.Name}", nameof(obj));
        }
        
        static readonly Dictionary<Type, FieldInfo[]> fieldInfoCache = new();
        static readonly Dictionary<Type, bool> typeContainsClassCache = new();
        static readonly Dictionary<Type, bool> typeIsClassCache = new();

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

        private static long GetFieldsMemorySize(object obj, FieldInfo[] fieldInfos, HashSet<object> seenObjects)
        {
            var totalSize = 0L;

            for (var index = 0; index < fieldInfos.Length; index++)
            {
                var fieldInfo = fieldInfos[index];
                var fieldValue = fieldInfo.GetValue(obj);
                totalSize += GetObjectSize(fieldValue, fieldInfo.FieldType, seenObjects);
            }

            return totalSize;
        }

        private static bool TypeIsClass(Type type)
        {
            if (!typeIsClassCache.TryGetValue(type, out var typeIsClass))
            {
                typeIsClassCache[type] = typeIsClass = type.IsClass;
            }

            return typeIsClass;
        }
        
        private static long GetObjectSize(object? obj, Type type, HashSet<object> seenObjects)
        {
            var typeIsClass = TypeIsClass(type);
            
            if (obj is not null && typeIsClass && seenObjects.Contains(obj))
            {
                return 0;
            }
            
            var size = GetSize(obj, seenObjects) + (type.IsClass ? PointerSize : 0);

            if (obj is not null && typeIsClass)
            {
                seenObjects.Add(obj);
            }
            
            return size;
        }
    }
}
