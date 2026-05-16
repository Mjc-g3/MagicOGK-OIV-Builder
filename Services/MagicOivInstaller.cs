using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Xml;
using CodeWalker.GameFiles;
using Microsoft.Win32;

namespace MagicOGK_OIV_Builder.Services
{
    public class MagicOivInstaller
    {
        public Action<string>? Log;

        // Old/simple call still works.
        public bool Install(string oivPath, string gta5Path)
        {
            InstallSummary summary = Install(oivPath, gta5Path, dryRun: false, progress: null);
            return summary.Errors == 0;
        }

        // New call with dry-run + progress + summary.
        public InstallSummary Install(
            string oivPath,
            string gta5Path,
            bool dryRun,
            IProgress<int>? progress)
        {
            string tempDir = Path.Combine(
                Path.GetTempPath(),
                "MagicOGK_" + Guid.NewGuid().ToString("N")
            );

            var summary = new InstallSummary
            {
                DryRun = dryRun,
                Gta5Path = gta5Path,
                OivPath = oivPath
            };

            var injectedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                progress?.Report(0);

                ValidateInputPaths(oivPath, gta5Path, summary);

                if (!dryRun)
                    ValidateNoConflictingProcesses();

                Log?.Invoke(dryRun ? "Starting dry-run preview..." : "Starting OIV install...");

                Log?.Invoke("Creating temp directory...");
                Directory.CreateDirectory(tempDir);

                Log?.Invoke("Extracting OIV package...");
                ZipFile.ExtractToDirectory(oivPath, tempDir);
                progress?.Report(10);

                string? assemblyPath = Directory
                    .GetFiles(tempDir, "assembly.xml", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (assemblyPath == null)
                {
                    AddIssue(summary, InstallErrorCategory.XmlError, "assembly.xml was not found.", oivPath);
                    return summary;
                }

                summary.AssemblyPath = assemblyPath;

                Log?.Invoke("assembly.xml located.");
                Log?.Invoke(assemblyPath);

                XmlDocument doc = new XmlDocument();
                doc.Load(assemblyPath);
                progress?.Report(20);

                string modName = ReadPackageName(doc, oivPath);
                summary.ModName = modName;

                string contentDir = Path.Combine(
                    Path.GetDirectoryName(assemblyPath)!,
                    "content"
                );

                XmlNodeList looseNodes = doc.SelectNodes("//content/add[@source]")!;
                XmlNodeList archiveNodes = doc.SelectNodes("//content/archive/add[@source]")!;

                ValidateUnsupportedXmlActions(doc, summary);

                int looseCount = looseNodes?.Count ?? 0;
                int archiveCount = archiveNodes?.Count ?? 0;
                int totalCount = looseCount + archiveCount;

                if (totalCount == 0)
                {
                    AddIssue(summary, InstallErrorCategory.XmlError, "No install operations were found in assembly.xml.", assemblyPath);
                    return summary;
                }

                summary.TotalOperations = totalCount;

                Log?.Invoke($"Found {totalCount} install operation(s).");
                progress?.Report(30);

                if (!dryRun && archiveCount > 0)
                    InitializeCodeWalker(gta5Path);

                int completed = 0;

                foreach (XmlNode node in looseNodes)
                {
                    completed++;
                    InstallLooseNode(node, contentDir, gta5Path, dryRun, injectedTargets, summary);
                    ReportOperationProgress(progress, completed, totalCount);
                }

                foreach (XmlNode node in archiveNodes)
                {
                    completed++;
                    InstallArchiveNode(node, contentDir, gta5Path, dryRun, injectedTargets, summary);
                    ReportOperationProgress(progress, completed, totalCount);
                }

                if (!dryRun && summary.Errors == 0)
                    SaveUninstallManifest(gta5Path, modName, summary);

                progress?.Report(100);

                Log?.Invoke(dryRun ? "Dry-run preview complete." : "Install complete.");
                Log?.Invoke($"Files installed/planned: {summary.FilesInstalled}");
                Log?.Invoke($"Skipped duplicates: {summary.SkippedDuplicates}");
                Log?.Invoke($"Warnings: {summary.Warnings}");
                Log?.Invoke($"Errors: {summary.Errors}");

                return summary;
            }
            catch (Exception ex)
            {
                AddIssue(summary, CategorizeError(ex), ex.Message, "");
                Log?.Invoke("ERROR:");
                Log?.Invoke(ex.Message);
                return summary;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Temp cleanup should never break install result.
                }
            }
        }

        public bool Uninstall(string gta5Path, string manifestPath, IProgress<int>? progress = null)
        {
            try
            {
                ValidateNoConflictingProcesses();

                if (!File.Exists(manifestPath))
                    throw new FileNotFoundException("Uninstall manifest not found.", manifestPath);

                Log?.Invoke("Reading uninstall manifest...");

                string json = File.ReadAllText(manifestPath);

                UninstallManifest? manifest =
                    System.Text.Json.JsonSerializer.Deserialize<UninstallManifest>(json);

                if (manifest == null)
                    throw new Exception("Could not read uninstall manifest.");

                int total = manifest.InstalledFiles.Count;
                int done = 0;

                foreach (string relativeFile in manifest.InstalledFiles)
                {
                    string fullPath = Path.Combine(
                        gta5Path,
                        relativeFile.Replace('/', Path.DirectorySeparatorChar)
                                    .Replace('\\', Path.DirectorySeparatorChar)
                    );

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        Log?.Invoke("Deleted: " + fullPath);
                    }
                    else
                    {
                        Log?.Invoke("Skipped missing file: " + fullPath);
                    }

                    done++;

                    if (total > 0)
                        progress?.Report((int)((done / (double)total) * 100));
                }

                progress?.Report(100);
                Log?.Invoke("Uninstall complete.");

                return true;
            }
            catch (Exception ex)
            {
                Log?.Invoke("UNINSTALL ERROR:");
                Log?.Invoke(ex.Message);
                return false;
            }
        }

        private void InstallLooseNode(
            XmlNode node,
            string contentDir,
            string gta5Path,
            bool dryRun,
            HashSet<string> injectedTargets,
            InstallSummary summary)
        {
            string source = node.Attributes?["source"]?.Value ?? "";
            string target = node.InnerText?.Trim() ?? "";

            Log?.Invoke($"SOURCE: {source}");
            Log?.Invoke($"TARGET: {target}");
            Log?.Invoke("");

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            {
                AddIssue(summary, InstallErrorCategory.XmlError, "Loose file operation is missing source or target.", target);
                return;
            }

            string duplicateKey = "loose:" + NormalizeGamePath(target);
            if (AlreadyInjected(injectedTargets, duplicateKey))
            {
                summary.SkippedDuplicates++;
                AddIssue(summary, InstallErrorCategory.DuplicateInjection, "Duplicate loose file injection skipped.", target, warningOnly: true);
                return;
            }

            string sourcePath = Path.Combine(
                contentDir,
                source.Replace('/', Path.DirectorySeparatorChar)
                      .Replace('\\', Path.DirectorySeparatorChar)
            );

            if (!File.Exists(sourcePath))
            {
                AddIssue(summary, InstallErrorCategory.FileError, "Source file missing: " + sourcePath, source);
                return;
            }

            string targetPath = Path.Combine(
                gta5Path,
                target.Replace('/', Path.DirectorySeparatorChar)
                      .Replace('\\', Path.DirectorySeparatorChar)
            );

            summary.FilesInstalled++;
            summary.InstalledFiles.Add(targetPath);

            if (dryRun)
            {
                Log?.Invoke("[DRY RUN] Would install loose file:");
                Log?.Invoke(targetPath);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(sourcePath, targetPath, true);

            Log?.Invoke("Installed loose file:");
            Log?.Invoke(targetPath);
        }

        private void InstallArchiveNode(
            XmlNode node,
            string contentDir,
            string gta5Path,
            bool dryRun,
            HashSet<string> injectedTargets,
            InstallSummary summary)
        {
            string source = node.Attributes?["source"]?.Value ?? "";
            string internalPath = node.InnerText?.Trim() ?? "";

            XmlNode archiveNode = node.ParentNode!;
            string rpfPath = archiveNode.Attributes?["path"]?.Value ?? "";

            Log?.Invoke($"SOURCE: {source}");
            Log?.Invoke($"RPF PATH: {rpfPath}");
            Log?.Invoke($"INTERNAL PATH: {internalPath}");
            Log?.Invoke("");

            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(rpfPath) ||
                string.IsNullOrWhiteSpace(internalPath))
            {
                AddIssue(summary, InstallErrorCategory.XmlError, "Archive operation is missing source, RPF path, or internal path.", rpfPath);
                return;
            }

            string duplicateKey =
                "rpf:" +
                NormalizeGamePath(rpfPath) +
                "::" +
                NormalizeGamePath(internalPath);

            if (AlreadyInjected(injectedTargets, duplicateKey))
            {
                summary.SkippedDuplicates++;
                AddIssue(summary, InstallErrorCategory.DuplicateInjection, "Duplicate RPF file injection skipped.", duplicateKey, warningOnly: true);
                return;
            }

            string sourcePath = Path.Combine(
                contentDir,
                source.Replace('/', Path.DirectorySeparatorChar)
                      .Replace('\\', Path.DirectorySeparatorChar)
            );

            if (!File.Exists(sourcePath))
            {
                AddIssue(summary, InstallErrorCategory.FileError, "Source file missing: " + sourcePath, source);
                return;
            }

            string targetRpfPath = Path.Combine(
                gta5Path,
                "mods",
                rpfPath.Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar)
            );

            summary.FilesInstalled++;
            summary.InstalledFiles.Add(targetRpfPath + "::" + internalPath);

            if (!summary.ModifiedXmlFiles.Contains(targetRpfPath, StringComparer.OrdinalIgnoreCase))
                summary.ModifiedXmlFiles.Add(targetRpfPath);

            if (dryRun)
            {
                Log?.Invoke("[DRY RUN] Would inject file into RPF:");
                Log?.Invoke(targetRpfPath);
                Log?.Invoke("Internal: " + internalPath);
                return;
            }

            byte[] fileData = File.ReadAllBytes(sourcePath);

            Log?.Invoke("RPF file ready for injection:");
            Log?.Invoke(targetRpfPath);
            Log?.Invoke("Internal: " + internalPath);

            InstallFileIntoRpf(gta5Path, rpfPath, internalPath, fileData);
        }

        private void ValidateUnsupportedXmlActions(XmlDocument doc, InstallSummary summary)
        {
            XmlNodeList unsupportedNodes = doc.SelectNodes(
                "//content/*[not(self::add) and not(self::archive)] | //content/archive/*[not(self::add)]"
            )!;

            if (unsupportedNodes == null || unsupportedNodes.Count == 0)
                return;

            Log?.Invoke($"WARNING: Found {unsupportedNodes.Count} unsupported OIV action(s).");

            foreach (XmlNode unsupported in unsupportedNodes)
            {
                string target = unsupported.InnerText?.Trim() ?? "";
                string operation = unsupported.Name;

                AddIssue(
                    summary,
                    InstallErrorCategory.UnsupportedOperation,
                    "Unsupported OIV/XML action: <" + operation + ">",
                    target,
                    warningOnly: true
                );

                Log?.Invoke("Unsupported action: <" + operation + ">");
            }

            Log?.Invoke("Some parts of this OIV may not install yet.");
        }

        private void ValidateInputPaths(string oivPath, string gta5Path, InstallSummary summary)
        {
            if (string.IsNullOrWhiteSpace(oivPath) || !File.Exists(oivPath))
            {
                AddIssue(summary, InstallErrorCategory.FileError, "OIV file does not exist.", oivPath);
                throw new FileNotFoundException("OIV file does not exist.", oivPath);
            }

            if (string.IsNullOrWhiteSpace(gta5Path) || !Directory.Exists(gta5Path))
            {
                AddIssue(summary, InstallErrorCategory.PathError, "GTA V path does not exist.", gta5Path);
                throw new DirectoryNotFoundException("GTA V path does not exist: " + gta5Path);
            }

            string gtaExe = Path.Combine(gta5Path, "GTA5.exe");
            if (!File.Exists(gtaExe))
            {
                AddIssue(summary, InstallErrorCategory.PathError, "GTA5.exe was not found in the selected GTA V folder.", gta5Path);
                throw new FileNotFoundException("GTA5.exe was not found in the selected GTA V folder.", gtaExe);
            }
        }

        private string ReadPackageName(XmlDocument doc, string oivPath)
        {
            string? name =
                doc.SelectSingleNode("//metadata/name")?.InnerText?.Trim() ??
                doc.SelectSingleNode("//name")?.InnerText?.Trim();

            if (!string.IsNullOrWhiteSpace(name))
                return MakeSafeFileName(name);

            return MakeSafeFileName(Path.GetFileNameWithoutExtension(oivPath));
        }

        private string MakeSafeFileName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return string.IsNullOrWhiteSpace(value) ? "Unknown_Mod" : value;
        }

        private void ReportOperationProgress(IProgress<int>? progress, int completed, int total)
        {
            if (total <= 0)
            {
                progress?.Report(100);
                return;
            }

            int percent = 30 + (int)Math.Round((completed / (double)total) * 65.0);
            percent = Math.Max(30, Math.Min(95, percent));
            progress?.Report(percent);
        }

        private bool TrySplitRpfTarget(
            string target,
            out string rpfPath,
            out string internalPath)
        {
            rpfPath = "";
            internalPath = "";

            target = target.Replace("\\", "/").TrimStart('/');

            int rpfIndex = target.IndexOf(".rpf/", StringComparison.OrdinalIgnoreCase);

            if (rpfIndex < 0)
                return false;

            int rpfEnd = rpfIndex + ".rpf".Length;

            rpfPath = target.Substring(0, rpfEnd);
            internalPath = target.Substring(rpfEnd + 1);

            return !string.IsNullOrWhiteSpace(rpfPath) &&
                   !string.IsNullOrWhiteSpace(internalPath);
        }

        private void InstallFileIntoRpf(
            string gta5Path,
            string rpfRelativePath,
            string internalPath,
            byte[] fileData)
        {
            string cleanRpfRelativePath = rpfRelativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            string originalRpfPath = Path.Combine(
                gta5Path,
                cleanRpfRelativePath
            );

            string modsRpfPath = Path.Combine(
                gta5Path,
                "mods",
                cleanRpfRelativePath
            );

            // Always write to mods folder, never original game RPF
            string fullRpfPath = modsRpfPath;

            if (!File.Exists(fullRpfPath))
            {
                if (!File.Exists(originalRpfPath))
                    throw new Exception("Original RPF not found: " + originalRpfPath);

                Directory.CreateDirectory(Path.GetDirectoryName(fullRpfPath)!);

                Log?.Invoke("Mods RPF not found.");
                Log?.Invoke("Copying original RPF into mods folder first...");
                Log?.Invoke("FROM: " + originalRpfPath);
                Log?.Invoke("TO: " + fullRpfPath);

                File.Copy(originalRpfPath, fullRpfPath, true);
            }
            else
            {
                Log?.Invoke("Using existing mods RPF:");
                Log?.Invoke(fullRpfPath);
            }

            if (!fullRpfPath.StartsWith(
                Path.Combine(gta5Path, "mods"),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Safety stop: attempted to write outside the mods folder.");
            }

            BackupRpf(gta5Path, rpfRelativePath);

            Log?.Invoke("Opening RPF:");
            Log?.Invoke(fullRpfPath);

            RpfFile rpf = new RpfFile(fullRpfPath, cleanRpfRelativePath);

            rpf.ScanStructure(
                msg => { },
                msg => Log?.Invoke(msg)
            );

            Log?.Invoke("RPF loaded.");

            string normalizedInternalPath = internalPath.Replace("\\", "/");

            string fileName = Path.GetFileName(normalizedInternalPath);

            string folderPath = Path.GetDirectoryName(normalizedInternalPath)?
                .Replace("\\", "/") ?? "";

            RpfDirectoryEntry dir = rpf.Root;

            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                string[] parts = folderPath.Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (string part in parts)
                {
                    RpfDirectoryEntry? nextDir = dir.Directories
                        .FirstOrDefault(d =>
                            d.Name.Equals(part, StringComparison.OrdinalIgnoreCase));

                    if (nextDir == null)
                        throw new Exception("Directory not found inside RPF: " + part);

                    dir = nextDir;
                }
            }

            Log?.Invoke("Injecting file:");
            Log?.Invoke(normalizedInternalPath);

            RpfFile.CreateFile(
                dir,
                fileName,
                fileData,
                true
            );

            Log?.Invoke("RPF file injected successfully.");
            Log?.Invoke("Close and reopen OpenIV if the file does not appear immediately.");
        }

        private void InitializeCodeWalker(string gta5Path)
        {
            Log?.Invoke("Initializing CodeWalker / GTA V keys...");

            GTA5Keys.LoadFromPath(gta5Path, false, null);

            string keysPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Keys"
            );

            GTA5Keys.LoadEncryptTablesFromPath(keysPath);

            GTA5Hash.Init();

            Log?.Invoke("CodeWalker initialized.");
        }

        private void BackupRpf(string gta5Path, string rpfPath)
        {
            string sourcePath = Path.Combine(
                gta5Path,
                "mods",
                rpfPath.Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar)
            );

            if (!File.Exists(sourcePath))
            {
                Log?.Invoke("Backup skipped, RPF does not exist yet:");
                Log?.Invoke(sourcePath);
                return;
            }

            string backupRoot = Path.Combine(
                gta5Path,
                "mods_backup",
                "MagicOGK",
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
            );

            string backupPath = Path.Combine(
                backupRoot,
                rpfPath.Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar)
            );

            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

            File.Copy(sourcePath, backupPath, true);

            Log?.Invoke("RPF backup created:");
            Log?.Invoke(backupPath);
        }

        private bool IsProcessRunning(string processName)
        {
            return System.Diagnostics.Process
                .GetProcessesByName(processName)
                .Length > 0;
        }

        private void ValidateNoConflictingProcesses()
        {
            string[] blockedProcesses =
            {
                "GTA5",
                "PlayGTAV",
                "OpenIV"
            };

            foreach (string process in blockedProcesses)
            {
                if (IsProcessRunning(process))
                {
                    throw new Exception(
                        $"Please close {process} before installing mods."
                    );
                }
            }
        }

        private void AddIssue(
            InstallSummary summary,
            InstallErrorCategory category,
            string message,
            string targetPath,
            bool warningOnly = false)
        {
            if (warningOnly)
                summary.Warnings++;
            else
                summary.Errors++;

            summary.Issues.Add(new InstallIssue
            {
                Category = category,
                Message = message,
                TargetPath = targetPath ?? ""
            });

            Log?.Invoke((warningOnly ? "WARNING: " : "ERROR: ") + message);

            if (!string.IsNullOrWhiteSpace(targetPath))
                Log?.Invoke("Target: " + targetPath);
        }

        private InstallErrorCategory CategorizeError(Exception ex)
        {
            string msg = ex.Message.ToLowerInvariant();

            if (msg.Contains("rpf")) return InstallErrorCategory.RpfError;
            if (msg.Contains("xml")) return InstallErrorCategory.XmlError;
            if (msg.Contains("unauthorized") || msg.Contains("access denied")) return InstallErrorCategory.PermissionError;
            if (msg.Contains("path")) return InstallErrorCategory.PathError;
            if (msg.Contains("duplicate")) return InstallErrorCategory.DuplicateInjection;
            if (ex is FileNotFoundException || ex is IOException) return InstallErrorCategory.FileError;
            if (ex is XmlException) return InstallErrorCategory.XmlError;
            if (ex is UnauthorizedAccessException) return InstallErrorCategory.PermissionError;

            return InstallErrorCategory.Unknown;
        }

        private bool AlreadyInjected(HashSet<string> installedTargets, string targetPath)
        {
            string normalized = NormalizeGamePath(targetPath);
            return !installedTargets.Add(normalized);
        }

        private string NormalizeGamePath(string path)
        {
            return (path ?? "")
                .Replace("\\", "/")
                .Trim()
                .TrimStart('/')
                .ToLowerInvariant();
        }

        private void SaveUninstallManifest(string gtaPath, string modName, InstallSummary summary)
        {
            var manifest = new UninstallManifest
            {
                ModName = modName,
                InstalledFiles = summary.InstalledFiles,
                ModifiedXmlFiles = summary.ModifiedXmlFiles
            };

            string dir = Path.Combine(gtaPath, "MagicOGK_UninstallLogs");
            Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, modName + "_manifest.json");

            string json = JsonSerializer.Serialize(
                manifest,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, json);

            Log?.Invoke("Uninstall manifest saved:");
            Log?.Invoke(path);
        }

        public static string? DetectGtaVPath()
        {
            string[] registryKeys =
            {
                @"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V",
                @"SOFTWARE\Rockstar Games\Grand Theft Auto V"
            };

            foreach (string keyPath in registryKeys)
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
                string? path = key?.GetValue("InstallFolder") as string;

                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    return path;
            }

            string? steamPath = DetectSteamGtaPath();
            if (!string.IsNullOrWhiteSpace(steamPath))
                return steamPath;

            string? epicPath = DetectEpicGtaPath();
            if (!string.IsNullOrWhiteSpace(epicPath))
                return epicPath;

            return null;
        }

        private static string? DetectSteamGtaPath()
        {
            string steamKey = @"SOFTWARE\WOW6432Node\Valve\Steam";

            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(steamKey);
            string? steamPath = key?.GetValue("InstallPath") as string;

            if (string.IsNullOrWhiteSpace(steamPath))
                return null;

            string defaultPath = Path.Combine(
                steamPath,
                @"steamapps\common\Grand Theft Auto V");

            if (Directory.Exists(defaultPath))
                return defaultPath;

            string libraryFile = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(libraryFile))
                return null;

            foreach (string line in File.ReadAllLines(libraryFile))
            {
                string trimmed = line.Trim();

                if (!trimmed.Contains("\"path\""))
                    continue;

                string[] parts = trimmed.Split('"', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                    continue;

                string libraryPath = parts[3].Replace(@"\\", @"\");

                string gtaPath = Path.Combine(
                    libraryPath,
                    @"steamapps\common\Grand Theft Auto V");

                if (Directory.Exists(gtaPath))
                    return gtaPath;
            }

            return null;
        }

        private static string? DetectEpicGtaPath()
        {
            string manifestsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Epic\EpicGamesLauncher\Data\Manifests"
            );

            if (!Directory.Exists(manifestsPath))
                return null;

            foreach (string file in Directory.GetFiles(manifestsPath, "*.item"))
            {
                string text = File.ReadAllText(file);

                if (!text.Contains("Grand Theft Auto V", StringComparison.OrdinalIgnoreCase) &&
                    !text.Contains("GTA", StringComparison.OrdinalIgnoreCase))
                    continue;

                const string installLocationToken = "\"InstallLocation\"";
                int tokenIndex = text.IndexOf(installLocationToken, StringComparison.OrdinalIgnoreCase);
                if (tokenIndex < 0)
                    continue;

                int colonIndex = text.IndexOf(':', tokenIndex);
                int firstQuote = text.IndexOf('"', colonIndex + 1);
                int secondQuote = text.IndexOf('"', firstQuote + 1);

                if (firstQuote < 0 || secondQuote < 0)
                    continue;

                string installPath = text.Substring(firstQuote + 1, secondQuote - firstQuote - 1)
                    .Replace(@"\\", @"\");

                if (Directory.Exists(installPath))
                    return installPath;
            }

            return null;
        }

        public enum InstallErrorCategory
        {
            None,
            RpfError,
            XmlError,
            FileError,
            PathError,
            UnsupportedOperation,
            DuplicateInjection,
            PermissionError,
            Unknown
        }

        public class InstallIssue
        {
            public InstallErrorCategory Category { get; set; }
            public string Message { get; set; } = "";
            public string TargetPath { get; set; } = "";
        }

        public class InstallSummary
        {
            public bool DryRun { get; set; }
            public string ModName { get; set; } = "";
            public string OivPath { get; set; } = "";
            public string Gta5Path { get; set; } = "";
            public string AssemblyPath { get; set; } = "";
            public int TotalOperations { get; set; }
            public int FilesInstalled { get; set; }
            public int XmlPatchesApplied { get; set; }
            public int SkippedDuplicates { get; set; }
            public int Warnings { get; set; }
            public int Errors { get; set; }

            public List<string> InstalledFiles { get; set; } = new();
            public List<string> ModifiedXmlFiles { get; set; } = new();
            public List<InstallIssue> Issues { get; set; } = new();

            public bool Success => Errors == 0;
        }

        public class UninstallManifest
        {
            public DateTime InstalledAt { get; set; } = DateTime.Now;
            public string ModName { get; set; } = "";
            public List<string> InstalledFiles { get; set; } = new();
            public List<string> ModifiedXmlFiles { get; set; } = new();
        }
    }
}