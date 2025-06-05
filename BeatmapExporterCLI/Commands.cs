using BeatmapExporterCLI.Data;
using BeatmapExporterCLI.Interface;
using BeatmapExporterCore.Exporters;
using BeatmapExporterCore.Filters;
using BeatmapExporterCore.Utilities;

namespace BeatmapExporterCLI;

internal static class Commands
{
    // Issues
    
    // Unable to pass refactorable references to enums, unlucky
    
    // optional [Argument] works very weirdly with optional options. '--filters "[]"' means '[]' is an argument.
    // So cant specify gameFolder as an argument and not a flag

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameFolder">Game location, if not standard</param>
    /// <param name="exportFormat">If set, the app works without user input. Options: Beatmap, Audio, Background, Replay, Folder, CollectionDb</param>
    /// <param name="exportPath">Location to export to.</param>
    /// <param name="filters">Filter rules to apply. Set as '["stars 6.3", "bpm 180"]' (Quotes are important). Run "--filters []" to know more.</param>
    /// <param name="matchAllFilters">If filters should be applied with AND logic where beatmaps must match all filters.</param>
    /// <param name="mergeCollections">Merges collections into an existing collection.db at the export location</param>
    /// <param name="mergeCaseInsensitive">Collections with the same name with different capitalization are merged</param>
    /// <param name="compressionEnabled">Only for Beatmap format. Slow export, smaller file sizes</param>
    public static void Do(string? gameFolder = null, ExportFormat? exportFormat = null, string? exportPath = null,
        string[]? filters = null, bool? matchAllFilters = null, bool? mergeCollections = null,
        bool? mergeCaseInsensitive = null, bool? compressionEnabled = null)
    {
        if (filters?.Length == 0)
        {
            Console.WriteLine(@"Only beatmaps which match ALL ACTIVE FILTERS will be exported.
Prefixing the filter with ""!"" will negate the filter, if you want to use a ""less than"" filter. ""!"" can be used with all filters, though an example is only shown for star rating.

Examples:
- To only export beatmaps 6.3 stars and above: stars 6.3
- Below 6.3 stars (negation example, works for all filters): !stars 6.3
- Longer than 1:30 (90 seconds): length 90
- 180BPM and above: bpm 180
- Beatmaps added in the last 7 days: since 7
- Beatmaps added in the last 5 hours: since 5:00
- Beatmaps ranked in the last 30 days: ranked 30
- Specific beatmap ID (comma-separated): id 1
- Mapped by RLC or Nathan (comma-separated): author RLC, Nathan
- Specific artists (comma-separated): artist Camellia, nanahira
- Tags include ""touhou"": tag touhou
- Specific gamemodes: mode osu/mania/ctb/taiko
- Beatmap status: status graveyard/leaderboard/ranked/approved/qualified/loved
- Beatmap played in the last 30 days: played 30
- Beatmap has ever been played: everplayed yes
- Beatmap has ever been played (include all diffs in set): everplayed set
- Contained in a specific collection called ""songs"": collection songs
- Contained in a specific collection labeled #1 in the collection list: collection #1
- Remove a specific filter (using line number from list above): remove 1
- Remove all filters: reset
");
            return;
        }

        // currently only load lazer, can add interface for selecting osu stable here later
        ExporterApp app = LazerLoader.Load(gameFolder);

        ClientSettings settings = new ClientSettings();

        if (exportPath != null)
            settings.ExportPath = exportPath;
        if (matchAllFilters != null)
            settings.MatchAllFilters = matchAllFilters.Value;
        if (mergeCollections != null)
            settings.MergeCollections = mergeCollections.Value;
        if (mergeCaseInsensitive != null)
            settings.MergeCaseInsensitive = mergeCaseInsensitive.Value;
        if (compressionEnabled != null)
            app.Configuration.CompressionEnabled = compressionEnabled.Value;
        
        app.Configuration.ApplySettings(settings);

        if (filters != null)
        {
            foreach (string filterInput in filters)
            {
                BeatmapFilter? filter;
                try
                {
                    filter = new FilterParser(filterInput).Parse();
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine($"Filter input error: {ae.Message}");
                    return;
                }

                if (filter is null)
                {
                    Console.WriteLine($"Invalid filter '{filterInput}'.");
                    return;
                }

                app.Configuration.Filters.Add(filter);
            }

            static void CollectionFailure(string filter) => Console.WriteLine($"Unable to find collection: {filter}.");
            app.Exporter.UpdateSelectedBeatmaps(CollectionFailure);

            if (app.Exporter.Configuration.Filters.Count > 0)
            {
                Console.Write("----------------------\nCurrent beatmap filters:\n\n");
                Console.Write(app.CLI.FilterDetail());
                Console.Write(
                    $"\nMatched beatmap sets: {app.Exporter.SelectedBeatmapSetCount}/{app.Exporter.TotalBeatmapSetCount}\n\n");
            }
            else
            {
                Console.Write(
                    "\n\nThere are no active beatmap filters. ALL beatmaps currently selected for export.\n\n");
            }
        }

        if (exportFormat != null)
        {
            app.Configuration.ExportFormat = exportFormat.Value;

            Console.WriteLine(exportFormat.Value.Descriptor());

            app.DoExport();
        }
        else
        {
            app.StartApplicationLoop();
            
            ExporterApp.Exit();
        }
    }
}