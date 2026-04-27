using System;
using System.Collections.Generic;

namespace DebugPanel.Options
{
    /// <summary>
    /// Interface for any object that can supply options to the Options Tab.
    /// </summary>
    public interface IOptionContainer
    {
        IEnumerable<OptionDefinition> GetOptions();
        bool IsDynamic { get; }
        event Action<OptionDefinition> OptionAdded;
        event Action<OptionDefinition> OptionRemoved;
    }
}
