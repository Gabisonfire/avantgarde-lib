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

        /// <summary>
        /// Creates an Updater instance
        /// </summary>
        /// <param name="path">The application root path</param>
        /// <param name="updateAG">Update the Avantgarde binary</param>
        public Updater(string path, bool updateAG = false)
        {
            SettingsFile = Settings.Load();
            SettingsFile.OriginalAppPath = path;
            SettingsFile.UpdateAG = updateAG;
        }

        /// <summary>
        /// Checks for available updates by comparing the settings version with the file manifest.
        /// </summary>
        /// <returns>True if an update is found.</returns>
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

        /// <summary>
        /// Launch the update process.
        /// </summary>
        public void Update()
        {
            // Call avantgarde.bin.exe
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.WorkingDirectory = Path.Combine(SettingsFile.OriginalAppPath, "agbin");
            psi.Arguments = $"\"{SettingsFile.OriginalAppPath.TrimEnd(Path.DirectorySeparatorChar)}\""; // strip the ending '\' from the path so the argument is correctly quoted 
            psi.FileName = "avantgarde.bin.exe";
            Process.Start(psi);
            Environment.Exit(0);
        }

        void PrepareUpdater()
        {
            //Download avantgarde bin
            DownloadManager dm = new DownloadManager();
            string agbinDir = Path.Combine(SettingsFile.OriginalAppPath, "agbin/");
            if(SettingsFile.UpdateAG) // Delete the existing agbin directory so the lib will redownload.
            {
                if(Directory.Exists(agbinDir))
                {
                    Directory.Delete(agbinDir);
                }
            }
            Directory.CreateDirectory(agbinDir);

            if (!File.Exists(Path.Combine(agbinDir, "avantgarde.bin.exe")))
            {
                string pathToZip = Path.Combine(agbinDir, "agbin.zip");

                dm.DownloadRemoteFile(AGBIN_URL, pathToZip);
                ZipFile.ExtractToDirectory(pathToZip, agbinDir);
                File.Delete(pathToZip);
            }

            // Move settings and file manifest to the agbin directory for launch
            string pathToAgFiles = Path.Combine(agbinDir, Settings.FILES_FILENAME);
            string pathToAgSettings = Path.Combine(agbinDir, Settings.SETTINGS_FILENAME);

            if (File.Exists(pathToAgFiles))
            {
                File.Delete(pathToAgFiles);
            }

            File.Move(Path.Combine(SettingsFile.OriginalAppPath, Settings.FILES_FILENAME), pathToAgFiles);
            File.Copy(Path.Combine(SettingsFile.OriginalAppPath, Settings.SETTINGS_FILENAME), pathToAgSettings, true);                        
        }

        FileManifest GetFileManifest()
        {
            string pathToAgFiles = Path.Combine(SettingsFile.OriginalAppPath, Settings.FILES_FILENAME); 

            DownloadManager dm = new DownloadManager();
            Utils.Log("Getting file manifest...");
            string filesJson;
            if (SettingsFile.FileManifest.StartsWith("http"))
            {
                dm.DownloadRemoteFile(SettingsFile.FileManifest, pathToAgFiles);
                filesJson = File.ReadAllText(pathToAgFiles);
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
