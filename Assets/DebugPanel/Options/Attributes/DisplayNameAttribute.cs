using System;

namespace DebugPanel.Options
{
    /// <summary>Overrides the display name shown in the Options Tab.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; }
        public DisplayNameAttribute(string name) => Name = name;
    }
}
