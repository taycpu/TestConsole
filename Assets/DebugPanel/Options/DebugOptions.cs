using System.ComponentModel;

namespace DebugPanel.Options
{
    /// <summary>
    /// Partial class root for developer-defined debug options.
    /// Extend this in other files: DebugOptions.Gameplay.cs, DebugOptions.Debug.cs, etc.
    ///
    /// Access the singleton via DebugOptions.Current.
    /// </summary>
    public partial class DebugOptions : INotifyPropertyChanged
    {
        private static DebugOptions _current;
        public static DebugOptions Current => _current ??= new DebugOptions();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
