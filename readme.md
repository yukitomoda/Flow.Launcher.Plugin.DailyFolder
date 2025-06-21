# DailyFolder Plugin for [Flow Launcher](https://www.flowlauncher.com)

A plugin for Flow Launcher that allows you to quickly create, open and prune daily folders.

# Features

## Create Daily Folder

`tmp` to create a new daily folder and open it.

This will create a new folder with the current date as the name (e.g. `2025-06-21`), and open it in your file explorer.

If you specify a number after `tmp` (e.g. `tmp 10`), it will open the folder for the specified number days ago.

If you specify `tmp base`, it will open the base folder.

## Prune Old Folders

`tmp prune` to prune old daily folders.

This will delete all daily folders older than 30 days ago (this count is configurable).
