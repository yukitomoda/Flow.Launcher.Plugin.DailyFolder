using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.IO;

namespace Flow.Launcher.Plugin.DailyFolder
{
    /// <summary>
    /// Settings for the DailyFolder plugin.
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private string _basePath = Path.Combine(SpecialDirectories.MyDocuments, "DailyFolder");
        /// <summary>
        /// The base path where daily folders are stored.
        /// </summary>
        public string BasePath
        {
            get => _basePath;
            set
            {
                _basePath = value;
                OnPropertyChanged(nameof(BasePath));
            }
        }

        private int _pruneRetentionCount = 30;
        /// <summary>
        /// The default number of items to retain during pruning operations.
        /// </summary>
        public int PruneRetentionCount
        {
            get => _pruneRetentionCount;
            set
            {
                _pruneRetentionCount = value;
                OnPropertyChanged(nameof(PruneRetentionCount));
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
