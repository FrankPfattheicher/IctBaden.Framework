using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.Types
{
    /// <summary>
    /// This class will create an object of a given type and populate it with sample data.
    /// </summary>
    internal class ObjectGenerator
    {
        public const int DefaultCollectionSize = 1;

        private readonly int _usedCollectionSize;
        private readonly SimpleTypeObjectGenerator _simpleObjectGenerator = new SimpleTypeObjectGenerator();

        /// <summary>
        /// Constructs a generator with a maximum collection size
        /// </summary>
        /// <param name="collectionSize"></param>
        private ObjectGenerator(int collectionSize)
        {
            _usedCollectionSize = collectionSize;
        }

        /// <summary>
        /// Generates an object for a given type. 
        /// The type needs to be public and have settable public properties/fields. 
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="collectionSize">Size limit for generated collections.</param>
        /// <returns>An object of the given type.</returns>
        public static object Generate(Type type, int collectionSize = DefaultCollectionSize)
        {
            var generator = new ObjectGenerator(collectionSize);
            return generator.GenerateObject(type);
        }

        /// <summary>
        /// Generates an object for a given type. The type needs to be public, have a public default constructor and settable public properties/fields. Currently it supports the following types:
        /// Simple types: <see cref="int"/>, <see cref="string"/>, <see cref="Enum"/>, <see cref="DateTime"/>, <see cref="Uri"/>, etc.
        /// Complex types: POCO types.
        /// Nullables: <see cref="Nullable{T}"/>.
        /// Arrays: arrays of simple types or complex types.
        /// Key value pairs: <see cref="KeyValuePair{TKey,TValue}"/>
        /// Tuples: <see cref="Tuple{T1}"/>, <see cref="Tuple{T1,T2}"/>, etc
        /// Dictionaries: <see cref="IDictionary{TKey,TValue}"/> or anything deriving from <see cref="IDictionary{TKey,TValue}"/>.
        /// Collections: <see cref="IList{T}"/>, <see cref="IEnumerable{T}"/>, <see cref="ICollection{T}"/>, <see cref="IList"/>, <see cref="IEnumerable"/>, <see cref="ICollection"/> or anything deriving from <see cref="ICollection{T}"/> or <see cref="IList"/>.
        /// Queryables: <see cref="IQueryable"/>, <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An object of the given type.</returns>
        public object GenerateObject(Type type)
        {
            return GenerateObject(type, new Dictionary<Type, object>());
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we just want to return null if anything goes wrong.")]
        private object GenerateObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            try
            {
                if (_simpleObjectGenerator.CanGenerateObject(type))
                {
                    return _simpleObjectGenerator.GenerateObject(type);
                }

                if (type.IsArray)
                {
                    return GenerateArray(type, _usedCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IDictionary))
                {
                    return GenerateDictionary(typeof(Hashtable), _usedCollectionSize, createdObjectReferences);
                }

                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return GenerateDictionary(type, _usedCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IList) ||
                    type == typeof(IEnumerable) ||
                    type == typeof(ICollection))
                {
                    return GenerateCollection(typeof(ArrayList), _usedCollectionSize, createdObjectReferences);
                }

                if (type.FullName != null && type.FullName.StartsWith("System.Collections.Generic.List"))
                {
                    return GenerateCollection(type, _usedCollectionSize, createdObjectReferences);
                }

                if (type.IsGenericType)
                {
                    return GenerateGenericType(type, _usedCollectionSize, createdObjectReferences);
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    return GenerateCollection(type, _usedCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IQueryable))
                {
                    return GenerateQueryable(type, _usedCollectionSize, createdObjectReferences);
                }

                if (type.IsEnum)
                {
                    return GenerateEnum(type);
                }

                if (type.IsPublic || type.IsNestedPublic)
                {
                    return GenerateComplexObject(type, createdObjectReferences);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // Returns null if anything fails
                Trace.TraceError("GenerateObject({0}) failed: {1}", type.Name, ex.Message);
                return null;
            }

            Trace.TraceError("GenerateObject({0}) failed.", type.Name);
            return null;
        }

        private object GenerateGenericType(Type type, int collectionSize, Dictionary<Type, object> createdObjectReferences)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                return GenerateNullable(type, createdObjectReferences);
            }

            if (genericTypeDefinition == typeof(KeyValuePair<,>))
            {
                return GenerateKeyValuePair(type, createdObjectReferences);
            }

            if (IsTuple(genericTypeDefinition))
            {
                return GenerateTuple(type, createdObjectReferences);
            }

            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                if (genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(ICollection<>))
                {
                    var collectionType = typeof(List<>).MakeGenericType(genericArguments);
                    return GenerateCollection(collectionType, collectionSize, createdObjectReferences);
                }

                if (genericTypeDefinition == typeof(IQueryable<>))
                {
                    return GenerateQueryable(type, collectionSize, createdObjectReferences);
                }

                var closedCollectionType = typeof(ICollection<>).MakeGenericType(genericArguments[0]);
                if (closedCollectionType.IsAssignableFrom(type))
                {
                    return GenerateCollection(type, collectionSize, createdObjectReferences);
                }
            }

            if (genericArguments.Length == 2)
            {
                if (genericTypeDefinition == typeof(IDictionary<,>))
                {
                    var dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                    return GenerateDictionary(dictionaryType, collectionSize, createdObjectReferences);
                }

                var closedDictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
                if (closedDictionaryType.IsAssignableFrom(type))
                {
                    return GenerateDictionary(type, collectionSize, createdObjectReferences);
                }
            }

            if (type.IsPublic || type.IsNestedPublic)
            {
                return GenerateComplexObject(type, createdObjectReferences);
            }

            Trace.TraceError("GenerateGenericType({0}) failed.", type.Name);
            return null;
        }

        private object GenerateTuple(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            var genericArgs = type.GetGenericArguments();
            var parameterValues = new object[genericArgs.Length];
            var failedToCreateTuple = true;
            for (var i = 0; i < genericArgs.Length; i++)
            {
                parameterValues[i] = GenerateObject(genericArgs[i], createdObjectReferences);
                failedToCreateTuple &= parameterValues[i] == null;
            }
            if (failedToCreateTuple)
            {
                Trace.TraceError("GenerateTuple({0}) failed.", type.Name);
                return null;
            }
            var result = Activator.CreateInstance(type, parameterValues);
            return result;
        }

        private static bool IsTuple(Type genericTypeDefinition)
        {
            return genericTypeDefinition == typeof(Tuple<>) ||
                genericTypeDefinition == typeof(Tuple<,>) ||
                genericTypeDefinition == typeof(Tuple<,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,,,,>);
        }

        private object GenerateKeyValuePair(Type keyValuePairType, Dictionary<Type, object> createdObjectReferences)
        {
            var genericArgs = keyValuePairType.GetGenericArguments();
            var typeK = genericArgs[0];
            var typeV = genericArgs[1];
            var keyObject = GenerateObject(typeK, createdObjectReferences);
            var valueObject = GenerateObject(typeV, createdObjectReferences);
            if (keyObject == null && valueObject == null)
            {
                // Failed to create key and values
                Trace.TraceError("GenerateKeyValuePair({0},{1}) failed (Key).", typeK.Name, typeV.Namespace);
                return null;
            }
            var result = Activator.CreateInstance(keyValuePairType, keyObject, valueObject);
            return result;
        }

        private object GenerateArray(Type arrayType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var type = arrayType.GetElementType() ?? throw new InvalidOperationException();
            var result = Array.CreateInstance(type, size);
            var areAllElementsNull = true;
            for (var i = 0; i < size; i++)
            {
                var element = GenerateObject(type, createdObjectReferences);
                result.SetValue(element, i);
                areAllElementsNull &= element == null;
            }

            return areAllElementsNull ? null : result;
        }

        private object GenerateDictionary(Type dictionaryType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var typeK = typeof(object);
            var typeV = typeof(object);
            if (dictionaryType.IsGenericType)
            {
                var genericArgs = dictionaryType.GetGenericArguments();
                typeK = genericArgs[0];
                typeV = genericArgs[1];
            }

            var result = Activator.CreateInstance(dictionaryType);
            var addMethod = dictionaryType.GetMethod("Add") ?? dictionaryType.GetMethod("TryAdd");
            var containsMethod = dictionaryType.GetMethod("Contains") ?? dictionaryType.GetMethod("ContainsKey");
            for (var i = 0; i < size; i++)
            {
                var newKey = GenerateObject(typeK, createdObjectReferences);
                if (newKey == null)
                {
                    // Cannot generate a valid key
                    return null;
                }

                var containsKey = containsMethod != null && (bool)containsMethod.Invoke(result, new[] { newKey });
                if (containsKey)
                    continue;
                var newValue = GenerateObject(typeV, createdObjectReferences);
                addMethod?.Invoke(result, new[] {newKey, newValue});
            }

            return result;
        }

        private object GenerateEnum(Type enumType)
        {
            var possibleValues = Enum.GetValues(enumType);
            if (possibleValues.Length > 0)
            {
                return possibleValues.GetValue(0);
            }
            Trace.TraceError("GenerateEnum({0}) failed.", enumType.Name);
            return null;
        }

        private object GenerateQueryable(Type queryableType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var isGeneric = queryableType.IsGenericType;
            object list;
            if (isGeneric)
            {
                var listType = typeof(List<>).MakeGenericType(queryableType.GetGenericArguments());
                list = GenerateCollection(listType, size, createdObjectReferences);
            }
            else
            {
                list = GenerateArray(typeof(object[]), size, createdObjectReferences);
            }
            if (list == null)
            {
                return null;
            }
            if (isGeneric)
            {
                var argumentType = typeof(IEnumerable<>).MakeGenericType(queryableType.GetGenericArguments());
                var asQueryableMethod = typeof(Queryable).GetMethod("AsQueryable", new[] { argumentType });
                return asQueryableMethod?.Invoke(null, new[] { list });
            }

            return ((IEnumerable)list).AsQueryable();
        }

        private object GenerateCollection(Type collectionType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var type = collectionType.IsGenericType ?
                collectionType.GetGenericArguments()[0] :
                typeof(object);
            var result = Activator.CreateInstance(collectionType);
            var addMethod = collectionType.GetMethod("Add");
            var areAllElementsNull = true;
            for (var i = 0; i < size; i++)
            {
                var elementGenerator = new ObjectGenerator(Math.Min(2, _usedCollectionSize - 1));
                var element = elementGenerator.GenerateObject(type, createdObjectReferences);
                addMethod?.Invoke(result, new[] { element });
                areAllElementsNull &= element == null;
            }

            if (areAllElementsNull)
            {
                return null;
            }

            return result;
        }

        private object GenerateNullable(Type nullableType, Dictionary<Type, object> createdObjectReferences)
        {
            var type = nullableType.GetGenericArguments()[0];
            return GenerateObject(type, createdObjectReferences);
        }

        private object GenerateComplexObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            if (createdObjectReferences.TryGetValue(type, out var result))
            {
                // The object has been created already, just return it. This will handle the circular reference case.
                return result;
            }

            if (type.IsValueType)
            {
                result = Activator.CreateInstance(type);
            }
            else
            {
                var defaultCtor = type.GetConstructor(Type.EmptyTypes);
                if (defaultCtor != null)
                {
                    result = defaultCtor.Invoke(Array.Empty<object>());
                }
                else
                {
                    // Type doesn't have a default constructor
                    foreach (var ctor in type.GetConstructors())
                    {
                        var ctorParams = new List<object>();
                        foreach (var param in ctor.GetParameters())
                        {
                            var paramSample = GenerateObject(param.ParameterType);
                            if (paramSample != null)
                            {
                                ctorParams.Add(paramSample);
                            }
                            else
                            {
                                // Cannot create parameter type
                                ctorParams = null;
                                break;
                            }
                        }

                        if (ctorParams == null)
                            continue;

                        result = Activator.CreateInstance(type, ctorParams.ToArray());
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (result != null)
                        {
                            break;
                        }
                    }

                }

            }

            if (result != null)
            {
                createdObjectReferences.Add(type, result);
                SetPublicProperties(type, result, createdObjectReferences);
                SetPublicFields(type, result, createdObjectReferences);
            }
            else
            {
                Trace.TraceError("GenerateComplexObject({0}) failed.", type.Name);
            }
            return result;
        }

        private void SetPublicProperties(Type type, object obj, Dictionary<Type, object> createdObjectReferences)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    continue;

                var propertyValue = GenerateObject(property.PropertyType, createdObjectReferences);

                #region Extension to be able to render properties which have the WriteRawStringAsJsonConverter attribute set.

                if (propertyValue is string)
                {
                    propertyValue = "\"" + propertyValue + "\"";
                }

                #endregion

                property.SetValue(obj, propertyValue, null);
            }
        }

        private void SetPublicFields(Type type, object obj, Dictionary<Type, object> createdObjectReferences)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var fieldValue = GenerateObject(field.FieldType, createdObjectReferences);
                field.SetValue(obj, fieldValue);
            }
        }

        private class SimpleTypeObjectGenerator
        {
            private long _index;
            private static readonly Dictionary<Type, Func<long, object>> DefaultGenerators = InitializeGenerators();

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple type factories and cannot be split up.")]
            private static Dictionary<Type, Func<long, object>> InitializeGenerators()
            {
                return new Dictionary<Type, Func<long, object>>
                {
                    // ReSharper disable BuiltInTypeReferenceStyle
                    // ReSharper disable RedundantCast
                    { typeof(Boolean), _ => true },
                    { typeof(Byte), _ => (Byte)64 },
                    { typeof(Char), _ => (Char)65 },
                    { typeof(DateTime), _ => DateTime.Now },
                    { typeof(DateTimeOffset), _ => new DateTimeOffset(DateTime.Now) },
                    { typeof(DBNull), _ => DBNull.Value },
                    { typeof(Decimal), index => (Decimal)index },
                    { typeof(Double), index => (Double)(index + 0.1) },
                    { typeof(Guid), _ => Guid.NewGuid() },
                    { typeof(Int16), index => (Int16)(index % Int16.MaxValue) },
                    { typeof(Int32), index => (Int32)(index % Int32.MaxValue) },
                    { typeof(Int64), index => (Int64)index },
                    { typeof(Object), _ => new object() },
                    { typeof(SByte), _ => (SByte)64 },
                    { typeof(Single), index => (Single)(index + 0.1) },
                    { 
                        typeof(String), index => String.Format(CultureInfo.CurrentCulture, "string {0}", index)
                    },
                    { 
                        typeof(TimeSpan), _ => TimeSpan.FromTicks(1234567)
                    },
                    { typeof(UInt16), index => (UInt16)(index % UInt16.MaxValue) },
                    { typeof(UInt32), index => (UInt32)(index % UInt32.MaxValue) },
                    { typeof(UInt64), index => (UInt64)index },
                    { 
                        typeof(Uri), index => new Uri(String.Format(CultureInfo.CurrentCulture, "http://sample{0}.com", index))
                    },
                    // ReSharper restore RedundantCast
                    // ReSharper restore BuiltInTypeReferenceStyle
                };
            }

            public bool CanGenerateObject(Type type)
            {
                return DefaultGenerators.ContainsKey(type);
            }

            public object GenerateObject(Type type)
            {
                return DefaultGenerators[type](++_index);
            }
        }
    }
}
