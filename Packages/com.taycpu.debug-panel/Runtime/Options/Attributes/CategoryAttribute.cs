using System;

namespace DebugPanel.Options
{
    /// <summary>Groups an option under a named category in the Options Tab.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class CategoryAttribute : Attribute
    {
        public string Name { get; }
        public CategoryAttribute(string name) => Name = name;
    }
}
