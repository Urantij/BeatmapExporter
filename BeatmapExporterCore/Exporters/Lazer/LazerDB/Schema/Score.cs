using System.Text.Json.Serialization;
using BeatmapExporterCore.Utilities;
using Realms;

// Original schema source file Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Scoring/ScoreInfo.cs

    /// <summary>
    /// A realm model containing metadata for a single score.
    /// </summary>
    public class Score : RealmObject
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        [JsonIgnore]
        public Beatmap? BeatmapInfo { get; set; }
        public string BeatmapHash { get; set; } = string.Empty;
        public IList<RealmNamedFileUsage> Files { get; } = null!;
        public double Accuracy { get; set; }
        public DateTimeOffset Date { get; set; }
        public RealmUser User { get; set; } = null!;
        public string Mods { get; set; } = string.Empty;
        public string Statistics { get; set; } = string.Empty;
        public IList<int> Pauses { get; } = null!;
        public int Rank { get; set; }

        /// <summary>
        /// The version of the client this score was set using.
        /// Sourced from <see cref="OsuGameBase.Version"/> at the point of score submission.
        /// </summary>
        public string ClientVersion { get; set; } = string.Empty;
        public Ruleset Ruleset { get; set; } = null!;
        public string Hash { get; set; } = string.Empty;
        public bool DeletePending { get; set; }
        /// <summary>
        /// The total number of points awarded for the score.
        /// </summary>
        public long TotalScore { get; set; }
        /// <summary>
        /// The total number of points awarded for the score without including mod multipliers.
        /// </summary>
        /// <remarks>
        /// The purpose of this property is to enable future lossless rebalances of mod multipliers.
        /// </remarks>
        public long TotalScoreWithoutMods { get; set; }
        /// <summary>
        /// Used to preserve the total score for legacy scores.
        /// </summary>
        /// <remarks>
        /// Not populated if <see cref="IsLegacyScore"/> is <c>false</c>.
        /// </remarks>
        public long? LegacyTotalScore { get; set; }
        /// <summary>
        /// If background processing of this beatmap failed in some way, this flag will become <c>true</c>.
        /// Should be used to ensure we don't repeatedly attempt to reprocess the same scores each startup even though we already know they will fail.
        /// </summary>
        /// <remarks>
        /// See https://github.com/ppy/osu/issues/24301 for one example of how this can occur (missing beatmap file on disk).
        /// </remarks>
        public bool BackgroundReprocessingFailed { get; set; }
        public int MaxCombo { get; set; }
        public double? PP { get; set; }
        /// <summary>
        /// The online ID of this score.
        /// </summary>
        /// <remarks>
        /// In the osu-web database, this ID (if present) comes from the new <c>solo_scores</c> table.
        /// </remarks>
        [Indexed]
        public long OnlineID { get; set; } = -1;
        /// <summary>
        /// The legacy online ID of this score.
        /// </summary>
        /// <remarks>
        /// In the osu-web database, this ID (if present) comes from the legacy <c>osu_scores_*_high</c> tables.
        /// This ID is also stored to replays set on osu!stable.
        /// </remarks>
        [Indexed]
        public long LegacyOnlineID { get; set; } = -1;
        public int Combo { get; set; }
        /// <summary>
        /// Whether this <see cref="ScoreInfo"/> represents a legacy (osu!stable) score.
        /// </summary>
        public bool IsLegacyScore { get; set; }
        /// <summary>
        /// The version of processing applied to calculate total score as stored in the database.
        /// If this does not match <see cref="LegacyScoreEncoder.LATEST_VERSION"/>,
        /// the total score has not yet been updated to reflect the current scoring values.
        ///
        /// See <see cref="BackgroundDataStoreProcessor"/>'s conversion logic.
        /// </summary>
        /// <remarks>
        /// This may not match the version stored in the replay files.
        /// </remarks>
        public int TotalScoreVersion { get; set; } = 30000016;
        [MapTo("MaximumStatistics")]
        public string MaximumStatisticsJson { get; set; } = string.Empty;

        // Author kabii
        /// <summary>
        /// Produces the output-friendly letter rank for this player score
        /// </summary>
        [Ignored] [JsonIgnore]
        public string RankLetter
        {
            get => Rank switch
            {
                -1 => "F",
                0 => "D",
                1 => "C",
                2 => "B",
                3 => "A",
                4 => "S",
                5 => "S+",
                6 => "SS",
                7 => "SS+",
                _ => "_"
            };
        }

        /// <summary>
        /// A string which distinguishes this score replay in a single beatmap set
        /// </summary>
        public string Details()
        {
            var age = DateTime.Now - Date;
            return $"({age.Days}d) {User.Username} {Accuracy:0.00%} {RankLetter} rank on [{BeatmapInfo!.DifficultyName}]";
        }

        /// <summary>
        /// The full filename to be used for exporting this player score replay.
        /// </summary>
        public string OutputReplayFilename() => 
            $"{User.Username} {RankLetter} rank on {BeatmapInfo!.Metadata.OutputName()} [{BeatmapInfo.DifficultyName}] ({Date.LocalDateTime:yyyy-MM-dd_HH-mm}).osr"
            .RemoveFilenameCharacters();
    }
}
