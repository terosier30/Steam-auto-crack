﻿using Serilog;
using System.IO.Compression;
using System.Reflection;
using System.Diagnostics;
using static SteamKit2.DepotManifest;

namespace SteamAutoCrack.Core.Utils
{
    public class GenCrackOnlyConfig
    {
        /// <summary>
        /// Game path to generate crack only files.
        /// </summary>
        public string? SourcePath { get; set; } = String.Empty;
        /// <summary>
        /// Crack only file output path.
        /// </summary>
        public string? OutputPath { get; set; } = Config.Config.TempPath;
        /// <summary>
        /// Create crack only readme file.
        /// </summary>
        public bool CreateReadme { get; set; } = false;
        /// <summary>
        /// Pack Crack only file with .zip archive.
        /// </summary>
        public bool Pack { get; set; } = false;
    }

    public class GenCrackOnlyConfigDefault
    {
        /// <summary>
        /// Game path to generate crack only files.
        /// </summary>
        public static readonly string? SourcePath = String.Empty;
        /// <summary>
        /// Crack only file output path.
        /// </summary>
        public static readonly string? OutputPath = Config.Config.TempPath;
        /// <summary>
        /// Create crack only readme file.
        /// </summary>
        public static readonly bool CreateReadme = false;
        /// <summary>
        /// Pack Crack only file with .zip archive.
        /// </summary>
        public static readonly bool Pack = false;
    }

    public interface IGenCrackOnly
    {
        public Task<bool> Applier(GenCrackOnlyConfig config);
    }

    public class GenCrackOnly : IGenCrackOnly
    {

        private readonly ILogger _log;
        public GenCrackOnly()
        {
            _log = Log.ForContext<GenCrackOnly>();
        }
        public async Task<bool> Applier(GenCrackOnlyConfig config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.SourcePath) || !Directory.Exists(config.SourcePath))
                {
                    _log.Error("Invaild input path.");
                    return false;
                }
                if (string.IsNullOrEmpty(config.OutputPath) || !Directory.Exists(config.OutputPath))
                {
                    _log.Error("Invaild output path.");
                    return false;
                }

                _log.Debug("Generating Crack only file...");
                if (Directory.Exists(Path.Combine(config.OutputPath, "Crack")))
                {
                    _log.Debug("Deleting exist output crack folder...");
                    Directory.Delete(Path.Combine(config.OutputPath, "Crack"), true);
                }
                if (File.Exists(Path.Combine(config.OutputPath, "Crack_Readme.txt")))
                {
                    _log.Debug("Deleting exist Crack_Readme.txt...");
                    File.Delete(Path.Combine(config.OutputPath, "Crack_Readme.txt"));
                }
                if (File.Exists(Path.Combine(config.OutputPath, "Crack.zip")))
                {
                    _log.Debug("Deleting exist Crack.zip...");
                    File.Delete(Path.Combine(config.OutputPath, "Crack.zip"));
                }
                Directory.CreateDirectory(Path.Combine(config.OutputPath, "Crack"));
                List<string> files = new List<string>();
                List<string> folders = new List<string>();
                foreach (string path in Directory.EnumerateFiles(config.SourcePath, "*.exe.bak", SearchOption.AllDirectories))
                {
                    files.Add(path);
                }
                

                foreach (string path in Directory.EnumerateFiles(config.SourcePath, "*.dll.bak", SearchOption.AllDirectories))
                {
                    files.Add(path);
                }

                foreach (string path in Directory.EnumerateDirectories(config.SourcePath, "steam_settings", SearchOption.AllDirectories))
                {
                    folders.Add(path);
                }

                foreach (string path in files)
                {
                    var origpath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                    if (!Directory.Exists(Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, Path.GetDirectoryName(path)))))
                    {
                        Directory.CreateDirectory(Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, Path.GetDirectoryName(path))));
                    }
                    if (!Directory.Exists(Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, Path.GetDirectoryName(origpath)))))
                    {
                        Directory.CreateDirectory(Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, Path.GetDirectoryName(path))));
                    }
                    File.Copy(path, Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, path)));
                    File.Copy(path, Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, origpath)));
                }
                foreach (string path in folders)
                {
                    CopyDirectory(new DirectoryInfo(path), new DirectoryInfo(Path.Combine(config.OutputPath, "Crack", Path.GetRelativePath(config.SourcePath, path))));
                }
                if (config.CreateReadme)
                {
                    File.AppendAllText(Path.Combine(config.OutputPath, "Crack_Readme.txt"), "Crack Generated By SteamAutoCrack " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    File.AppendAllText(Path.Combine(config.OutputPath, "Crack_Readme.txt"), "\n1. Copy All File in Crack Folder to Game Folder, then Overwrite.");
                }
                if (config.Pack)
                {
                    ZipFile.CreateFromDirectory(Path.Combine(config.OutputPath, "Crack"), Path.Combine(config.OutputPath, "Crack.zip"));
                    if (config.CreateReadme)
                    {
                        var filestream = new FileStream(Path.Combine(config.OutputPath, "Crack.zip"), FileMode.Open);
                        new ZipArchive(filestream, ZipArchiveMode.Update).CreateEntryFromFile(Path.Combine(config.OutputPath, "Crack_Readme.txt"), "Crack_Readme.txt");
                        filestream.Close();
                    }
                }
                _log.Information("Crack only file generated.");
                return true;
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to generate crack only file.");
                return false;
            }

        }
        public void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
