using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeatmapExporterCore.Exporters.Lazer.LazerDB;
using BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema;
using Realms;

namespace BeatmapAntiDeadge;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        LazerDatabase fromDb = new(args[0]);
        LazerDatabase toDb = new(args[1]);

        Realm fromRealm = fromDb.Open(@readonly: false);
        
        Transaction fromRealmT = fromRealm.BeginWrite();
        
        var cancer = fromRealm.Find<BeatmapSet>(new Guid("2da99b8e-0308-4f70-aecf-7d980c7cca54"));
        
        cancer.Beatmaps[2].Metadata.UserTags.Clear();
        
        fromRealmT.Commit();
        fromRealmT.Dispose();
        return;
        
        Realm toRealm = toDb.Open(@readonly: false);
        
        // Unhandled exception. Realms.Exceptions.RealmInvalidTransactionException: Cannot modify managed List outside of a write transaction.
        // void Set(object obj, string name, object? value)
        // {
        //     var ff = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        //     var f = ff.First(f => f.Name == name);
        //     f.SetValue(obj, value);
        // }
        // var cancer = fromRealm.Find<BeatmapSet>(new Guid("2da99b8e-0308-4f70-aecf-7d980c7cca54"));
        // var bcancer = cancer.Beatmaps[2];
        // var mcancer = bcancer.Metadata;
        // Set(mcancer, "<UserTags>k__BackingField", new List<string>());
        //
        // var ff = bcancer.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        // Set(bcancer, "<Metadata>k__BackingField", null);
        // Если сделать поле юзертегов нулл, он при сериализации всё равно падает насмерть.
        // только если самому клирнуть лист юзертегов, всё будет нормально.
        // который кстати можно клирнуть только в рв моде :) во время транзакции... хуй знает.
        
        

        int counter = 0;

        Transaction transaction = toRealm.BeginWrite();

        var j = new JsonSerializerOptions()
        {
    ReferenceHandler = ReferenceHandler.Preserve,
        };

        foreach (BeatmapSet beatmapSet in fromRealm.All<BeatmapSet>())
        {
            foreach (Beatmap beatmap in beatmapSet.Beatmaps)
            {
                if (beatmap.BeatmapSet?.ID != beatmapSet.ID)
                {
                    Console.WriteLine($"{beatmap.BeatmapSet?.ID} vs {beatmapSet.ID}");
                    return;
                }
            }
            
            Console.Write($"{beatmapSet.ID}");
        
            if (beatmapSet.ID.ToString() == "2da99b8e-0308-4f70-aecf-7d980c7cca54")
            {
                Console.Write(" GNIDA");
                // beatmapSet.Beatmaps[2].Metadata.UserTags.Clear();
                
                // var bcancer = beatmapSet.Beatmaps[2];
                // var mcancer = bcancer.Metadata;
                // var ff = mcancer.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                // var f = ff.First(f => f.Name == "<UserTags>k__BackingField");
                // f.SetValue(mcancer, new List<string>());
                
                Console.WriteLine("fuh");
            }
        
            BeatmapSet b;
            try
            {
                var a = JsonSerializer.Serialize<BeatmapSet>(beatmapSet, options: j);
                b = JsonSerializer.Deserialize<BeatmapSet>(a, options: j)!;
            }
            catch (Exception e)
            {
                Console.WriteLine($"CANCER {beatmapSet.ID} {beatmapSet.ArchiveFilename()}");
                continue;
            }
            
            Console.Write(" 1");
        
            foreach (Beatmap beatmap in b.Beatmaps)
            {
                beatmap.BeatmapSet = b;
        
                foreach (Score score in beatmap.Scores)
                {
                    score.BeatmapInfo = beatmap;
                }
            }
            
            toRealm.Add(b);
            
            counter++;
            if (counter > 1)
            {
                counter = 0;
                try
                {
                    transaction.Commit();
                }
                catch (System.AccessViolationException)
                {
                    Console.WriteLine($"CANCER2 {beatmapSet.ID} {beatmapSet.ArchiveFilename()}");
                }
                transaction.Dispose();
                
                transaction = toRealm.BeginWrite();
            }
            
            Console.Write("2\n");
        }
    }

    static void myway(string db)
    {
        LazerDatabase fromDb = new(db);

        Realm fromRealm = fromDb.Open(@readonly: false);

        var j = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
        };
        
        var cancer = fromRealm.Find<BeatmapSet>(new Guid("2da99b8e-0308-4f70-aecf-7d980c7cca54"));
        
        // cancer.AllScores.Clear();
        // cancer.Beatmaps.Clear();
        // cancer.Files.Clear();

        var uebishe = cancer.Beatmaps[2];
        
        cancer.Beatmaps.Remove(uebishe);
        
        // cancer.Beatmaps.RemoveAt(0);
        // cancer.Beatmaps.RemoveAt(0);
        // cancer.Beatmaps.RemoveAt(0);
        // cancer.Beatmaps.RemoveAt(0);
        
        //

        // uebishe.Difficulty = null;
        // uebishe.Ruleset = null;
        // uebishe.UserSettings = null;
        //
        // uebishe.BeatmapSet = null;
        
        // fatal error tut
        // uebishe.Metadata = null;

        var death = uebishe.Metadata.UserTags.ToList();
    }
}