using System.ComponentModel;

namespace DebugPanel.Options
{
    /// <summary>
    /// Default options container registered by the package bootstrapper.
    /// Package consumers usually register their own objects with DebugPanelManager.Instance.AddOptionContainer.
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
