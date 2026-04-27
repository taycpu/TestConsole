using System;
using System.Collections.Generic;

namespace DebugPanel.Options
{
    /// <summary>
    /// A runtime-mutable container of OptionDefinitions.
    /// Add/remove options at will and the Options Tab will update accordingly.
    /// </summary>
    public class DynamicOptionContainer : IOptionContainer
    {
        private readonly List<OptionDefinition> _options = new List<OptionDefinition>();

        public bool IsDynamic => true;

        public event Action<OptionDefinition> OptionAdded;
        public event Action<OptionDefinition> OptionRemoved;

        public IEnumerable<OptionDefinition> GetOptions() => _options;

        public void AddOption(OptionDefinition option)
        {
            _options.Add(option);
            OptionAdded?.Invoke(option);
        }

        public void RemoveOption(OptionDefinition option)
        {
            if (_options.Remove(option))
                OptionRemoved?.Invoke(option);
        }

        public void Clear()
        {
            var copy = new List<OptionDefinition>(_options);
            foreach (var opt in copy)
                RemoveOption(opt);
        }
    }
}
