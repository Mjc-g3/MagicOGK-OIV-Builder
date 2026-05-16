using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using CodeWalker.GameFiles;

namespace MagicOGK_OIV_Builder.Services
{
    public class MagicOivInstaller
    {
        public Action<string>? Log;

        public bool Install(string oivPath, string gta5Path)
        {
            string tempDir = Path.Combine(
                Path.GetTempPath(),
                "MagicOGK_" + Guid.NewGuid().ToString("N")
            );

            try
            {
                ValidateNoConflictingProcesses();

                Log?.Invoke("Creating temp directory...");
                Directory.CreateDirectory(tempDir);

                Log?.Invoke("Extracting OIV package...");
                ZipFile.ExtractToDirectory(oivPath, tempDir);

                string assemblyPath = Directory
                    .GetFiles(tempDir, "assembly.xml", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (assemblyPath == null)
                {
                    Log?.Invoke("assembly.xml was not found.");
                    return false;
                }

                Log?.Invoke("assembly.xml located.");
                Log?.Invoke(assemblyPath);

                InitializeCodeWalker(gta5Path);

                XmlDocument doc = new XmlDocument();
                doc.Load(assemblyPath);

                XmlNodeList looseNodes = doc.SelectNodes("//content/add[@source]");
                XmlNodeList archiveNodes = doc.SelectNodes("//content/archive/add[@source]");

                int looseCount = looseNodes?.Count ?? 0;
                int archiveCount = archiveNodes?.Count ?? 0;
                int totalCount = looseCount + archiveCount;

                XmlNodeList unsupportedNodes = doc.SelectNodes(
    "//content/*[not(self::add) and not(self::archive)] | //content/archive/*[not(self::add)]"
);

                if (unsupportedNodes != null && unsupportedNodes.Count > 0)
                {
                    Log?.Invoke($"WARNING: Found {unsupportedNodes.Count} unsupported OIV action(s).");

                    foreach (XmlNode unsupported in unsupportedNodes)
                    {
                        Log?.Invoke("Unsupported action: <" + unsupported.Name + ">");
                    }

                    Log?.Invoke("Some parts of this OIV may not install yet.");
                }

                if (totalCount == 0)
                {
                    Log?.Invoke("No install operations found.");
                    return false;
                }

                Log?.Invoke($"Found {totalCount} install operation(s).");

                string contentDir = Path.Combine(
                    Path.GetDirectoryName(assemblyPath)!,
                    "content"
                );

                // ── Loose files, example: <content><add source="">target</add></content>
                foreach (XmlNode node in looseNodes)
                {
                    string source = node.Attributes["source"]?.Value ?? "";
                    string target = node.InnerText?.Trim() ?? "";

                    Log?.Invoke($"SOURCE: {source}");
                    Log?.Invoke($"TARGET: {target}");
                    Log?.Invoke("");

                    string sourcePath = Path.Combine(
                        contentDir,
                        source.Replace('/', Path.DirectorySeparatorChar)
                              .Replace('\\', Path.DirectorySeparatorChar)
                    );

                    if (!File.Exists(sourcePath))
                    {
                        Log?.Invoke("Source file missing: " + sourcePath);
                        continue;
                    }

                    string targetPath = Path.Combine(
                        gta5Path,
                        target.Replace('/', Path.DirectorySeparatorChar)
                              .Replace('\\', Path.DirectorySeparatorChar)
                    );

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    File.Copy(sourcePath, targetPath, true);

                    Log?.Invoke("Installed loose file:");
                    Log?.Invoke(targetPath);
                }

                // ── RPF/archive files, example: <archive path="update/update.rpf"><add>...</add></archive>
                foreach (XmlNode node in archiveNodes)
                {
                    string source = node.Attributes["source"]?.Value ?? "";
                    string internalPath = node.InnerText?.Trim() ?? "";

                    XmlNode archiveNode = node.ParentNode!;
                    string rpfPath = archiveNode.Attributes?["path"]?.Value ?? "";

                    Log?.Invoke($"SOURCE: {source}");
                    Log?.Invoke($"RPF PATH: {rpfPath}");
                    Log?.Invoke($"INTERNAL PATH: {internalPath}");
                    Log?.Invoke("");

                    string sourcePath = Path.Combine(
                        contentDir,
                        source.Replace('/', Path.DirectorySeparatorChar)
                              .Replace('\\', Path.DirectorySeparatorChar)
                    );

                    if (!File.Exists(sourcePath))
                    {
                        Log?.Invoke("Source file missing: " + sourcePath);
                        continue;
                    }

                    byte[] fileData = File.ReadAllBytes(sourcePath);

                    Log?.Invoke("RPF file ready for injection:");
                    Log?.Invoke(Path.Combine(gta5Path, rpfPath));
                    Log?.Invoke("Internal: " + internalPath);

                    // Actual RPF write comes next
                    InstallFileIntoRpf(gta5Path, rpfPath, internalPath, fileData);
                }

                Log?.Invoke("Package parse complete.");

                return true;
            }

            catch (Exception ex)
            {
                Log?.Invoke("ERROR:");
                Log?.Invoke(ex.Message);
                return false;
            }

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

            if (!fullRpfPath.Contains(
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
                    RpfDirectoryEntry nextDir = dir.Directories
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

        //backup fucntion on .rpf writing
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

        //Block .rpf reading/writing if game or OpenIV is running, to prevent corruption
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
    }
}