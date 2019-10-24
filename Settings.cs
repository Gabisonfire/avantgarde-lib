using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Avantgarde.Lib
{
    public class Settings
    {
        public const string SETTINGS_FILENAME = "ag_settings.json";
        public const string FILES_FILENAME = "ag_files.json";
        public bool IsArchive { get; set; }
        public bool Relaunch { get; set; }
        public string ExeName{ get; set; }
        public string CloseMethod { get; set; } // kill, close, waitAndKill, none
        public string TempDir { get; set; }
        public string FileManifest { get; set; }
        public string CurrentVersion { get; set; } // Need read Assembly Info Alternative

        [JsonIgnore]
        public string OriginalAppPath { get; set; }
        [JsonIgnore]
        public bool UpdateAG { get; set; }
   
        public static Settings Load(string path = SETTINGS_FILENAME)
        {
            if(!File.Exists(path))
            {
                Utils.Log("Cannot find settings.json", Utils.MsgType.error);
                Environment.Exit(2);
            }
            Settings settings = null;
            try
            {
                string cfg = File.ReadAllText(path);
                settings = JsonConvert.DeserializeObject<Settings>(cfg);
            }
            catch(Exception ex)
            {
                Utils.Log(ex.Message, Utils.MsgType.error);
            }
            Utils.Log("Settings loaded.");
            return settings;
        }

        public static void Save(Settings settings, string path = SETTINGS_FILENAME)
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
                Utils.Log("Settings saved.");
            }
            catch (Exception ex)
            {
                Utils.Log(ex.Message, Utils.MsgType.error);
            }
        }

    }
}
