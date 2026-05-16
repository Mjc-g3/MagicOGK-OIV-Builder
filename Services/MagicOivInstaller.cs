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

            string fullRpfPath = Path.Combine(
    gta5Path,
    "mods",
    cleanRpfRelativePath
);

            string originalRpfPath = Path.Combine(gta5Path, cleanRpfRelativePath);

            if (!File.Exists(fullRpfPath))
            {
                if (!File.Exists(originalRpfPath))
                    throw new Exception("Original RPF not found: " + originalRpfPath);

                Directory.CreateDirectory(Path.GetDirectoryName(fullRpfPath)!);

                Log?.Invoke("Mods RPF not found. Copying original RPF to mods folder...");
                File.Copy(originalRpfPath, fullRpfPath, true);
            }

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

            if (GTA5Keys.PC_NG_ENCRYPT_TABLES == null ||
                GTA5Keys.PC_NG_ENCRYPT_LUTs == null)
            {
                throw new Exception("Encryption tables were not loaded from Keys folder.");
            }

            Log?.Invoke("CodeWalker initialized.");
        }
    }
}