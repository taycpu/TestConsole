using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DebugPanel.Options
{
    /// <summary>
    /// Reflects over a plain C# object and produces OptionDefinitions
    /// for all public properties and void/no-arg public methods,
    /// respecting Category, NumberRange, DisplayName, and Sort attributes.
    /// </summary>
    public static class OptionContainerScanner
    {
        private static readonly HashSet<Type> SupportedPropertyTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(int),
            typeof(uint),
            typeof(short),
            typeof(ushort),
            typeof(byte),
            typeof(sbyte),
            typeof(float),
            typeof(double),
            typeof(string)
        };

        public static List<OptionDefinition> Scan(object container)
        {
            var results = new List<OptionDefinition>();
            if (container == null) return results;

            var type = container.GetType();

            // ── Properties ────────────────────────────────────────────────
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propType = prop.PropertyType;
                bool isSupported = SupportedPropertyTypes.Contains(propType) || propType.IsEnum;
                if (!isSupported) continue;

                var category = GetCategory(prop);
                var displayName = GetDisplayName(prop, prop.Name);
                var sortOrder = GetSort(prop);
                var range = prop.GetCustomAttribute<NumberRangeAttribute>();

                var capturedProp = prop;
                var capturedContainer = container;

                Func<object> getter = () =>
                {
                    try { return capturedProp.GetValue(capturedContainer); }
                    catch (Exception e) { Debug.LogError($"[DebugPanel] Getter error on {capturedProp.Name}: {e.Message}"); return null; }
                };

                Action<object> setter = null;
                if (prop.CanWrite && prop.GetSetMethod() != null)
                {
                    setter = value =>
                    {
                        try
                        {
                            object converted = Convert.ChangeType(value, capturedProp.PropertyType);
                            capturedProp.SetValue(capturedContainer, converted);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[DebugPanel] Setter error on {capturedProp.Name}: {e.Message}");
                        }
                    };
                }

                var def = new OptionDefinitionBuilder()
                    .WithName(displayName)
                    .WithCategory(category)
                    .WithSortOrder(sortOrder)
                    .WithValueType(propType)
                    .WithGetter(getter)
                    .WithSetter(setter)
                    .WithRange(range?.Min, range?.Max)
                    .Build();

                results.Add(def);
            }

            // ── Methods ───────────────────────────────────────────────────
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.ReturnType != typeof(void)) continue;
                if (method.GetParameters().Length != 0) continue;
                if (method.IsSpecialName) continue; // skip property backing methods

                var category = GetCategory(method);
                var displayName = GetDisplayName(method, method.Name);
                var sortOrder = GetSort(method);

                var capturedMethod = method;
                var capturedContainer = container;

                var def = OptionDefinition.FromMethod(
                    displayName,
                    () => capturedMethod.Invoke(capturedContainer, null),
                    category,
                    sortOrder
                );

                results.Add(def);
            }

            return results;
        }

        private static string GetCategory(MemberInfo member)
        {
            return member.GetCustomAttribute<CategoryAttribute>()?.Name ?? "General";
        }

        private static string GetDisplayName(MemberInfo member, string fallback)
        {
            return member.GetCustomAttribute<DisplayNameAttribute>()?.Name
                ?? SplitCamelCase(fallback);
        }

        private static int GetSort(MemberInfo member)
        {
            return member.GetCustomAttribute<SortAttribute>()?.Order ?? 0;
        }

        private static string SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = System.Text.RegularExpressions.Regex.Replace(input, "(\\B[A-Z])", " $1");
            return result;
        }
    }

    /// <summary>Internal builder to construct OptionDefinition via reflection data.</summary>
    internal class OptionDefinitionBuilder
    {
        private string _name;
        private string _category = "General";
        private int _sortOrder;
        private Type _valueType;
        private Func<object> _getter;
        private Action<object> _setter;
        private float? _rangeMin;
        private float? _rangeMax;
        private Action _methodAction;

        public OptionDefinitionBuilder WithName(string name) { _name = name; return this; }
        public OptionDefinitionBuilder WithCategory(string cat) { _category = cat; return this; }
        public OptionDefinitionBuilder WithSortOrder(int order) { _sortOrder = order; return this; }
        public OptionDefinitionBuilder WithValueType(Type t) { _valueType = t; return this; }
        public OptionDefinitionBuilder WithGetter(Func<object> g) { _getter = g; return this; }
        public OptionDefinitionBuilder WithSetter(Action<object> s) { _setter = s; return this; }
        public OptionDefinitionBuilder WithRange(float? min, float? max) { _rangeMin = min; _rangeMax = max; return this; }
        public OptionDefinitionBuilder WithMethod(Action a) { _methodAction = a; return this; }

        public OptionDefinition Build()
        {
            // Use reflection to set private fields since OptionDefinition constructor is private
            var def = (OptionDefinition)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(OptionDefinition));

            Set(def, "Name", _name);
            Set(def, "Category", _category);
            Set(def, "SortOrder", _sortOrder);
            Set(def, "ValueType", _valueType);
            Set(def, "Getter", _getter);
            Set(def, "Setter", _setter);
            Set(def, "RangeMin", _rangeMin);
            Set(def, "RangeMax", _rangeMax);
            Set(def, "MethodAction", _methodAction);

            return def;
        }

        private static void Set(object obj, string propName, object value)
        {
            var prop = typeof(OptionDefinition).GetProperty(propName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(obj, value);
        }
    }
}
