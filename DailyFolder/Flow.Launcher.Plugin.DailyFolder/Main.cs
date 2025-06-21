using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.DailyFolder.Views;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.DailyFolder
{
    /// <summary>
    /// Create and open a daily folder based on the current date.
    /// </summary>
    public class DailyFolder : IPlugin, ISettingProvider
    {
        private PluginInitContext _context;
        private Settings _settings;

        /// <inheritdoc/>
        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
            _settings.PropertyChanged += (sender, _) =>
            {
                context.API.SaveSettingJsonStorage<Settings>();
            };
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
            else if (query.FirstSearch == "prune")
            {
                int retentionCount;
                if (int.TryParse(query.SecondSearch, out int retentionCountSpec) && 0 <= retentionCountSpec)
                {
                    retentionCount = retentionCountSpec;
                }
                else
                {
                    retentionCount = _settings.PruneDefaultRetentionCount;
                }

                var entriesToDelete = entries.Skip(retentionCount).ToList();

                results.Add(new Result
                {
                    Score = 1000,
                    Title = $"Prune Daily Folders",
                    SubTitle = $"Prune all but the {retentionCount} newest folders.",
                    Action = _ =>
                    {
                        int deletedCount = 0;
                        foreach (var (dir, date) in entriesToDelete)
                        {
                            if (DeleteFolderRecursive(dir)) deletedCount++;
                        }

                        _context.API.ShowMsg("Prune Daily Folder", $"{deletedCount} folders deleted.");
                        return true;
                    },
                });

            }

            results.Add(new Result()
            {
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

        private bool DeleteFolderRecursive(string path)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore even if it does not exist
                return false;
            }
            catch (Exception)
            {
                _context.API.ShowMsgError("Failed to Daily Folder",
                    $"Failed to delete folder {path}.");
                return false;
            }
        }

        private static string GetDailyFolderName(DateTime now)
        {
            return $"{now:yyyy-MM-dd}";
        }

        /// <inheritdoc/>
        public Control CreateSettingPanel()
        {
            return new SettingsPanel(_settings);
        }
    }
}