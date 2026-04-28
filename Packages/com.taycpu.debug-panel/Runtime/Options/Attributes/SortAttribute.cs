using System;

namespace DebugPanel.Options
{
    /// <summary>Controls the sort order of an option within its category.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class SortAttribute : Attribute
    {
        public int Order { get; }
        public SortAttribute(int order) => Order = order;
    }
}
