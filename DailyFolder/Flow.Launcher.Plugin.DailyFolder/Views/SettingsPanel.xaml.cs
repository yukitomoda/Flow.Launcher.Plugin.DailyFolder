using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.DailyFolder.Views
{
    /// <summary>
    /// A user interface for the settings panel of the Daily Folder plugin.
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPanel"/> class with the specified settings.
        /// </summary>
        /// <param name="settings">The settings object used to configure the panel.</param>
        public SettingsPanel(Settings settings)
        {
            InitializeComponent();
            DataContext = settings;
        }
    }
}