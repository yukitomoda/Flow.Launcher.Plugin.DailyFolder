using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace Flow.Launcher.Plugin.DailyFolder
{
    internal class Settings
    {
        public string BasePath = Path.Combine(SpecialDirectories.MyDocuments, "DailyFolder");

        public int EntriesCount = 3;

        public int PruneDefaultRetentionCount = 10;
    }
}
