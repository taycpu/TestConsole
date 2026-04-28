using System;
using System.Collections.Generic;

namespace DebugPanel.Options
{
    /// <summary>
    /// Describes a single option entry in the Options Tab.
    /// Can represent a property (with getter/setter) or a method (action button).
    /// </summary>
    public class OptionDefinition
    {
        public string Name { get; private set; }
        public string Category { get; private set; }
        public int SortOrder { get; private set; }
        public Type ValueType { get; private set; }

        // For property options
        public Func<object> Getter { get; private set; }
        public Action<object> Setter { get; private set; }
        public bool IsReadOnly => Setter == null;

        // For method options
        public Action MethodAction { get; private set; }
        public bool IsMethod => MethodAction != null;

        // Optional metadata
        public float? RangeMin { get; private set; }
        public float? RangeMax { get; private set; }

        private OptionDefinition() { }

        // ── Factory: typed property ────────────────────────────────────────

        public static OptionDefinition Create<T>(
            string name,
            Func<T> getter,
            Action<T> setter = null,
            string category = "Default",
            int sortPriority = 0,
            float? rangeMin = null,
            float? rangeMax = null)
        {
            return new OptionDefinition
            {
                Name = name,
                Category = category,
                SortOrder = sortPriority,
                ValueType = typeof(T),
                Getter = () => getter(),
                Setter = setter != null ? (Action<object>)(v => setter((T)v)) : null,
                RangeMin = rangeMin,
                RangeMax = rangeMax
            };
        }

        // ── Factory: method / button ───────────────────────────────────────

        public static OptionDefinition FromMethod(
            string name,
            Action action,
            string category = "Default",
            int sortPriority = 0)
        {
            return new OptionDefinition
            {
                Name = name,
                Category = category,
                SortOrder = sortPriority,
                ValueType = typeof(void),
                MethodAction = action
            };
        }
    }
}
