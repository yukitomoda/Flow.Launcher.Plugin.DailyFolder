using Flow.Launcher.Plugin.DailyFolder.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

            if (int.TryParse(query.Search, out int count) && 0 <= count && count < 10000)
            {
                var date = now.AddDays(-count);
                results.Add(new Result
                {
                    Score = 1000,
                    Title = $"Open Daily Folder of {count} days ago",
                    SubTitle = $"Open the daily folder for {count} days ago({date:yyyy-MM-dd}). Create if it does not exist.",
                    IcoPath = "assets/icon.png",
                    Action = _ =>
                    {
                        var path = EnsureDailyFoldeExists(date);
                        return TryOpenDirectory(path);
                    },
                });
            }
            else if (query.FirstSearch == "prune")
            {

                var entriesToDelete = EnumerateDailyFolderPaths()
                    .OrderByDescending(entry => entry.date)
                    .Skip(_settings.PruneRetentionCount).ToList();

                results.Add(new Result
                {
                    Score = 1000,
                    Title = $"Prune Old Daily Folders",
                    SubTitle = $"Prune old daily folders, retaining the {_settings.PruneRetentionCount} newest ones.",
                    IcoPath = "assets/icon.png",
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
            else if (query.FirstSearch == "base")
            {
                results.Add(new Result()
                {
                    Score = 1000,
                    Title = "Open Base Folder",
                    SubTitle = $"Open the base folder of daily folders.",
                    IcoPath = "assets/icon.png",
                    Action = _ =>
                    {
                        var path = EnsureBaseFolderExists();
                        return TryOpenDirectory(path);
                    },
                });
            }

            results.Add(new Result()
            {
                Score = 0,
                Title = "Open Daily Folder",
                SubTitle = $"Open the daily folder for today({now:yyyy-MM-dd}). Create if it does not exist.",
                IcoPath = "assets/icon.png",
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