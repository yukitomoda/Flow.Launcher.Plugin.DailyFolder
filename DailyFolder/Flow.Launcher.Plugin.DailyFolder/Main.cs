using Flow.Launcher.Plugin;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        private Settings _settings;

        /// <inheritdoc/>
        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
        }

        /// <inheritdoc/>
        public List<Result> Query(Query query)
        {
            Settings settings = _context.API.LoadSettingJsonStorage<Settings>();
            var results = new List<Result>();
            DateTime now = DateTime.Now;

            var entries = EnumerateDailyFolderPaths()
                .OrderByDescending(entry => entry.date)
                .ToList();

            if (int.TryParse(query.Search, out int count) && 0 <= count && count < entries.Count)
            {
                results.AddRange(entries.Skip(count).Take(_settings.EntriesCount)
                    .Select((entry, i) => new Result
                    {
                        Score = 1000 - i,
                        Title = $"Open Daily Folder @{entry.date:yyyy-MM-dd}",
                        SubTitle = $"Back {count + i} folders from the latest.",
                        Action = _ =>
                        {
                            return TryOpenDirectory(entry.dir);
                        },
                    }));
            }

            results.Add(new Result() {
                Score = 0,
                Title = "Open Daily Folder",
                SubTitle = $"Open the daily folder for today({now:yyyy-MM-dd}). Create if it does not exist.",
                Action = _ =>
                {
                    var path = EnsureDailyFoldeExists(now);
                    return TryOpenDirectory(path);
                },
            });

            return results;
        }

        /// <summary>
        /// Create a base folder if not exists. Do nothing if it already exists.
        /// </summary>
        /// <returns>The folder path to be created.</returns>
        private string EnsureBaseFolderExists()
        {
            Directory.CreateDirectory(_settings.BasePath);
            return _settings.BasePath;
        }

        private string EnsureDailyFoldeExists(DateTime now)
        {
            var folderName = GetDailyFolderName(now);
            var basePath = EnsureBaseFolderExists();
            var path = Path.Combine(basePath, folderName);
            Directory.CreateDirectory(path);
            return path;
        }

        private IEnumerable<(string dir, DateTime date)> EnumerateDailyFolderPaths()
        {
            var basePath = EnsureBaseFolderExists();
            var dirs = Directory.GetDirectories(basePath, "*", SearchOption.TopDirectoryOnly);

            return dirs.Select(dir =>
            {
                var dirName = Path.GetFileName(dir);
                if (DateTime.TryParseExact(dirName, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return (dir, date: (DateTime?)date);
                }
                return (dir, date: null);
            })
                .Where(e => e.date != null)
                .Select(e => (e.dir, e.date.Value));
        }

        private bool TryOpenDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                _context.API.ShowMsgError("Folder Not Found", $"The folder {path} does not exist.");
                return false;
            }

            _context.API.OpenDirectory(path);
            return true;
        }

        private static string GetDailyFolderName(DateTime now)
        {
            return $"{now:yyyy-MM-dd}";
        }
    }
}