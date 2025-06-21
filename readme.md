# DailyFolder Plugin for [Flow Launcher](https://www.flowlauncher.com)

A plugin for Flow Launcher that allows you to quickly create, open and prune daily folders.

# Features

## Open Daily Folder

```txt
# open the folder for today
df

# open the folder for yesterday
df 1

# open the folder for 10 days ago
df 10

# open the base folder
df base
```

`df` to create a new daily folder and open it.

This will create a new folder with the current date as the name (e.g. `2025-06-21`), and open it in your file explorer.

If you specify a number after `df` (e.g. `df 10`), it will open the folder for the specified number days ago.

If you specify `df base`, it will open the base folder.

## Prune Old Folders

```txt
# prune old daily folders
df prune
```

`df prune` to prune old daily folders.

This will delete all daily folders older than 30 days ago (this count is configurable).
