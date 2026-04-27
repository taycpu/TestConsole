using System;

namespace DebugPanel.Options
{
    /// <summary>Clamps a numeric property to [Min, Max] in the Options Tab slider.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NumberRangeAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }
        public NumberRangeAttribute(float min, float max) { Min = min; Max = max; }
    }
}
