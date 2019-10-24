using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Avantgarde.Lib;
using System.IO.Compression;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Avantgarde.Core
{
    public class Updater
    {
        Settings SettingsFile { get; }

        FileManifest Manifest { get; set; }

        const string AGBIN_URL = "https://github.com/Gabisonfire/avantgarde-bin/releases/latest/download/agbin.zip";

        public Updater(string path, bool updateAG = false)
        {
            SettingsFile = Settings.Load();
            SettingsFile.OriginalAppPath = path;
            SettingsFile.UpdateAG = updateAG;
        }

        public bool CheckForUpdates()
        {            
            Manifest = GetFileManifest();
            PrepareUpdater();
            Utils.Log($"Current version is: {SettingsFile.CurrentVersion}");
            Utils.Log($"Available version is: {Manifest.TargetVersion}");
            Version Target = new Version(Manifest.TargetVersion);
            Version Current = new Version(SettingsFile.CurrentVersion);            
            if (Target > Current)
            {
                Utils.Log($"Found {Manifest.Files.Count.ToString()} file(s) to update.");
                return true;
            }
            else
            {
                Utils.Log($"No update available.");
                return false;
            }
            
        }

        public void Update()
        {
            // Call avantgarde.bin.exe
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.WorkingDirectory = SettingsFile.OriginalAppPath + "agbin";
            psi.Arguments = SettingsFile.OriginalAppPath;
            psi.FileName = "avantgarde.bin.exe";
            Process.Start(psi);
            Environment.Exit(0);
        }

        void PrepareUpdater()
        {
            //Download avantgarde bin
            DownloadManager dm = new DownloadManager();
            string agbinDir = SettingsFile.OriginalAppPath + "agbin/";
            if(SettingsFile.UpdateAG) // Delete the existing agbin directory so the lib will redownload.
            {
                if(Directory.Exists(agbinDir))
                {
                    Directory.Delete(agbinDir);
                }
            }
            Directory.CreateDirectory(agbinDir);
            if (!File.Exists(agbinDir + "avantgarde.bin.exe"))
            {
                dm.DownloadRemoteFile(AGBIN_URL, agbinDir + "agbin.zip");
                ZipFile.ExtractToDirectory(agbinDir + "agbin.zip", agbinDir);
                File.Delete(agbinDir + "agbin.zip");
            }            
            // Move settings and file manifest to the agbin directory for launch
            File.Move(SettingsFile.OriginalAppPath + Settings.FILES_FILENAME, agbinDir + Settings.FILES_FILENAME, true);
            File.Copy(SettingsFile.OriginalAppPath + Settings.SETTINGS_FILENAME, agbinDir + Settings.SETTINGS_FILENAME, true);                        
        }

        FileManifest GetFileManifest()
        {
            DownloadManager dm = new DownloadManager();
            Utils.Log("Getting file manifest...");
            string filesJson;
            if (SettingsFile.FileManifest.StartsWith("http"))
            {
                dm.DownloadRemoteFile(SettingsFile.FileManifest, SettingsFile.OriginalAppPath + Settings.FILES_FILENAME);
                filesJson = File.ReadAllText(SettingsFile.OriginalAppPath + Settings.FILES_FILENAME);
            }
            else
            {
                filesJson = File.ReadAllText(SettingsFile.FileManifest);
            }
            Utils.Log("Reading file manifest...");
            return JsonConvert.DeserializeObject<FileManifest>(filesJson);
        }
    }


}
