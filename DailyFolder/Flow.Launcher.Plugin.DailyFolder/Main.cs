using Flow.Launcher.Plugin;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.DailyFolder
{
    /// <summary>
    /// Create and open a daily folder based on the current date.
    /// </summary>
    public class DailyFolder : IPlugin
    {
        private PluginInitContext _context;

        /// <inheritdoc/>
        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public List<Result> Query(Query query)
        {
            DateTime now = DateTime.Now;

            return new List<Result> {
                    new () {
                        Title = "Open Daily Folder",
                        SubTitle = $"Open the daily folder for {now:yyyy-MM-dd}. Create if it does not exist.",
                        Action = _ =>
                        {
                            var path = EnsureDailyFolderAsync(now);
                            _context.API.OpenDirectory(path);
                            return true;
                        },
                    }
                };
        }

        /// <summary>
        /// Create a base folder if not exists. Do nothing if it already exists.
        /// </summary>
        /// <returns>The folder path to be created.</returns>
        private string EnsureBaseFolderAsync()
        {
            Settings settings = _context.API.LoadSettingJsonStorage<Settings>();
            Directory.CreateDirectory(settings.BasePath);
            return settings.BasePath;
        }

        private string EnsureDailyFolderAsync(DateTime now)
        {
            var folderName = GetDailyFolderName(now);
            var basePath = EnsureBaseFolderAsync();
            var path = Path.Combine(basePath, folderName);
            Directory.CreateDirectory(path);
            return path;
        }

        private static string GetDailyFolderName(DateTime now)
        {
            return $"{now:yyyy-MM-dd}";
        }
    }
}