using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace MagicOGK_OIV_Builder
{
    public static class OIVBuilder
    {
        // ── Entry point ───────────────────────────────────────────────────────

        public static void Build(OIVProject project, string outputPath, IProgress<int>? progress = null)
        {
            foreach (var file in project.Files)
                if (!File.Exists(file.SourcePath))
                    throw new FileNotFoundException($"Source file not found: {file.SourcePath}");

            string tempDir = Path.Combine(Path.GetTempPath(), "MagicOGK_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            progress?.Report(0);

            try
            {
                /// ── content/ folder: files grouped by their resolved install path ─────────
                // Example:
                // content\update-x64-dlcpacks-mpgunrunning-dlc.rpf\dlc.rpf
                // content\update-update.rpf-common-data\dlclist.xml
                string contentDir = Path.Combine(tempDir, "content");
                Directory.CreateDirectory(contentDir);

                // source file path → relative path inside content/
                var sourceToContentName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var usedContentPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                List<string> originalPathLines = new();

                foreach (var file in project.Files)
                {
                    if (sourceToContentName.ContainsKey(file.SourcePath))
                        continue;

                    string installPath = ResolveInstallPath(file, project)
                        .Replace("/", "\\")
                        .Trim('\\');

                    string fileName = Path.GetFileName(installPath);

                    string? installFolder = Path.GetDirectoryName(installPath);

                    string contentFolderName = string.IsNullOrWhiteSpace(installFolder)
                        ? "_root"
                        : MakeFlatFolderName(installFolder);

                    string contentRelativePath = Path.Combine(contentFolderName, fileName);

                    contentRelativePath = MakeUniqueContentPath(contentRelativePath, usedContentPaths);
                    usedContentPaths.Add(contentRelativePath);

                    sourceToContentName[file.SourcePath] = contentRelativePath.Replace("\\", "/");

                    string destination = Path.Combine(contentDir, contentRelativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

                    File.Copy(file.SourcePath, destination, overwrite: true);

                    originalPathLines.Add($"{contentRelativePath.Replace("\\", "/")}  =>  {installPath}");
                }

                File.WriteAllLines(
                    Path.Combine(contentDir, "_original_install_paths.txt"),
                    originalPathLines,
                    new UTF8Encoding(false)
                );

                // ── icon.png in package root ──────────────────────────────────
                // Spec: optional, must be exactly 128×128 px, named icon.png at root.
                string iconPath = Path.Combine(tempDir, "icon.png");
                if (!string.IsNullOrWhiteSpace(project.PhotoPath) && File.Exists(project.PhotoPath))
                    CreateIcon128(project.PhotoPath, iconPath);
                else
                    CreateDefaultIcon(iconPath);

                progress?.Report(55);

                // ── assembly.xml ──────────────────────────────────────────────
                string assemblyXml = BuildAssemblyXml(project, sourceToContentName);
                File.WriteAllText(
                    Path.Combine(tempDir, "assembly.xml"),
                    assemblyXml,
                    new UTF8Encoding(false));

                progress?.Report(65);

                // ── Zip to .oiv ───────────────────────────────────────────────
                if (File.Exists(outputPath)) File.Delete(outputPath);
                CreateZipWithProgress(tempDir, outputPath, progress);
                progress?.Report(100);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        // ── assembly.xml generation ───────────────────────────────────────────

        private static string BuildAssemblyXml(
            OIVProject project,
            Dictionary<string, string> sourceToContentName)
        {
            var doc = new XmlDocument();
            doc.InsertBefore(
                doc.CreateXmlDeclaration("1.0", "UTF-8", null),
                doc.DocumentElement);

            // <package version="2.2" id="{GUID}" target="Five">
            var pkg = AppendElem(doc, doc, "package");
            pkg.SetAttribute("version", "2.2");
            pkg.SetAttribute("id",     "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            pkg.SetAttribute("target", "Five");

            WriteMetadata(doc, pkg, project);
            WriteColors(doc, pkg, project);
            WriteContent(doc, pkg, project, sourceToContentName);

            var sb = new StringBuilder();
            using (var sw = new StringWriterUtf8(sb))
            using (var w  = new XmlTextWriter(sw) { Formatting = Formatting.Indented, Indentation = 1, IndentChar = '\t' })
                doc.Save(w);
            return sb.ToString();
        }

        // ── <metadata> ────────────────────────────────────────────────────────

        private static void WriteMetadata(XmlDocument doc, XmlElement pkg, OIVProject p)
        {
            var meta = AppendElem(doc, pkg, "metadata");
            AppendElem(doc, meta, "name").InnerText = p.ModName;

            var ver = AppendElem(doc, meta, "version");
            AppendElem(doc, ver, "major").InnerText = GetVersionPart(p.Version, 0);
            AppendElem(doc, ver, "minor").InnerText = GetVersionPart(p.Version, 1);
            if (!string.IsNullOrWhiteSpace(p.VersionTag))
                AppendElem(doc, ver, "tag").InnerText = p.VersionTag.ToUpper();

            var author = AppendElem(doc, meta, "author");
            AppendElem(doc, author, "displayName").InnerText = p.Author;

            var desc = AppendElem(doc, meta, "description");
            desc.AppendChild(doc.CreateCDataSection(
                string.IsNullOrWhiteSpace(p.Description)
                    ? $"{p.ModName} by {p.Author}"
                    : p.Description));
        }

        // ── <colors> ─────────────────────────────────────────────────────────

        private static void WriteColors(XmlDocument doc, XmlElement pkg, OIVProject p)
        {
            Color hdr = string.IsNullOrWhiteSpace(p.BannerColor)
                ? Color.FromArgb(255, 35, 54, 106)
                : ParseHexColor(p.BannerColor);

            var colors = AppendElem(doc, pkg, "colors");

            var hdrBg = AppendElem(doc, colors, "headerBackground");
            hdrBg.SetAttribute("useBlackTextColor", "False");
            hdrBg.InnerText = ToArgbHex(hdr);

            AppendElem(doc, colors, "iconBackground").InnerText =
                ToArgbHex(Color.FromArgb(255, 59, 89, 152));
        }

        // ── <content> ────────────────────────────────────────────────────────
        //
        // The spec uses a recursive <archive> nesting model.
        // We reconstruct that tree from the project's flat folder list + file list.
        //
        // Key rules from spec:
        //   <add source="FileName.ext">install\path\FileName.ext</add>
        //     source = just the filename inside content/ (no subfolder)
        //     inner text = full install path in the game (may include subdirs inside the archive)
        //
        //   <archive path="full\path\to.rpf" createIfNotExist="True" type="RPF7">
        //     ... <add> or nested <archive> children ...
        //   </archive>
        //
        //   dlclist injection uses the inline <xml> command, no external patch files needed.

        private static void WriteContent(
            XmlDocument doc,
            XmlElement pkg,
            OIVProject project,
            Dictionary<string, string> sourceToContentName)
        {
            var content = AppendElem(doc, pkg, "content");

            // Separate files into RPF-bound vs loose
            // RPF-bound: the resolved install path contains a .rpf segment
            // Loose: installed directly into the game folder (scripts/, plugins/, etc.)

            var looseFiles = new List<OIVFileEntry>();
            // Map from top-level archive path → list of (file, path-inside-archive)
            // We need to build a proper nested structure.

            // Step 1: resolve every file's full install path
            var resolved = new Dictionary<OIVFileEntry, string>();
            foreach (var file in project.Files)
                resolved[file] = ResolveInstallPath(file, project);

            // Step 2: group by the outermost RPF in their path
            // e.g. "update\update.rpf\common\data\water.xml"
            //   → outermost RPF = "update\update.rpf"
            //   → path inside RPF = "common\data\water.xml"
            //
            // Nested archives (rpf inside rpf) are handled recursively below.

            foreach (var file in project.Files)
            {
                string path = resolved[file];
                string outerRpf = ExtractFirstRpfPath(path);
                if (string.IsNullOrEmpty(outerRpf))
                    looseFiles.Add(file);
                // RPF files are handled via the folder tree — see WriteArchiveNode
            }

            // Step 3: Write top-level archive nodes by recursing the folder tree
            // Root-level folders (ParentId == null) that are RPF nodes become top-level <archive> elements.
            // Non-RPF root folders contribute their path prefix but don't produce their own <archive> node
            // (the children do).
            WriteArchiveChildren(
                doc,
                content,
                project,
                project.Folders.Where(f => f.ParentId == null).ToList(),
                resolved,
                sourceToContentName,
                null);

            // Step 4: Write loose files (no RPF in path)
            foreach (var file in looseFiles)
            {
                string contentName = sourceToContentName[file.SourcePath];
                string installPath = resolved[file];
                var add = AppendElem(doc, content, "add");
                add.SetAttribute("source", contentName);
                add.InnerText = installPath;
            }

            // Step 5: dlclist.xml inline XML edits
            // Per spec: use <xml path="common\data\dlclist.xml"> with <add append="Last" xpath="...">
            // wrapped inside the update.rpf archive node.
            var dlcFolders = project.Folders.Where(f => f.AddToDlcList).ToList();
            if (dlcFolders.Count > 0)
            {
                // Check whether update.rpf is already in the tree; if not add it
                // We always emit a separate block for dlclist edits to keep it clean
                var updateRpf = AppendElem(doc, content, "archive");
                updateRpf.SetAttribute("path",             @"update\update.rpf");
                updateRpf.SetAttribute("createIfNotExist", "False");
                updateRpf.SetAttribute("type",             "RPF7");

                var xmlNode = AppendElem(doc, updateRpf, "xml");
                xmlNode.SetAttribute("path", @"common\data\dlclist.xml");

                foreach (var folder in dlcFolders)
                {
                    string dlcEntry = $"dlcpacks:/{folder.Name}/";
                    var addNode = AppendElem(doc, xmlNode, "add");
                    addNode.SetAttribute("append", "Last");
                    addNode.SetAttribute("xpath",  "/SMandatoryPacksData/Paths");
                    AppendElem(doc, addNode, "Item").InnerText = dlcEntry;
                }
            }
        }

        // Create zip with progress reporting. Progress is reported from 65 to 100% during this phase.
        private static void CreateZipWithProgress(string sourceFolder, string outputPath, IProgress<int>? progress)
        {
            string[] files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

            using FileStream zipStream = new FileStream(outputPath, FileMode.Create);
            using ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];

                string entryName = Path.GetRelativePath(sourceFolder, file)
                    .Replace("\\", "/");

                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);

                int percent = 65 + (int)(((i + 1) / (double)Math.Max(files.Length, 1)) * 35);
                progress?.Report(percent);
            }
        }

        // ── Path resolution ───────────────────────────────────────────────────

        // Resolves the full install path for a file by walking up the folder tree.
        // e.g. folders: update(non-rpf) → x64(non-rpf) → dlcpacks(non-rpf) → mod1(non-rpf) → dlc.rpf(rpf)
        // file: dlc.rpf → install path = "update\x64\dlcpacks\mod1\dlc.rpf\dlc.rpf" ← file at root of RPF
        // Actually the file sits INSIDE the rpf, so:
        // install path for the <add> inner text = file.FileName (path inside the RPF)
        // and the <archive path="..."> = "update\x64\dlcpacks\mod1\dlc.rpf"
        private static string ResolveInstallPath(OIVFileEntry file, OIVProject project)
        {

            var parts = new List<string>();

            int? cur = file.FolderId;
            while (cur.HasValue)
            {
                var folder = project.Folders.Find(f => f.Id == cur.Value);
                if (folder == null) break;
                parts.Insert(0, folder.Name);
                cur = folder.ParentId;
            }

            if (!string.IsNullOrWhiteSpace(file.SubPath))
            {
                foreach (var part in file.SubPath
                             .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    parts.Add(part);
                }
            }

            parts.Add(file.FileName);
            return string.Join("\\", parts);
        }



        // Returns the path of the outermost .rpf segment.
        // "update\update.rpf\common\data\water.xml" → "update\update.rpf"
        // "scripts\asi.asi" → ""
        private static string ExtractFirstRpfPath(string path)
        {
            string[] parts = path.Split('\\');
            var sb = new StringBuilder();
            foreach (string p in parts)
            {
                if (sb.Length > 0) sb.Append('\\');
                sb.Append(p);
                if (p.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    return sb.ToString();
            }
            return string.Empty;
        }
        // ── Icon ──────────────────────────────────────────────────────────────

        private static void CreateIcon128(string sourcePath, string destPng)
        {
            try
            {
                using var source = new Bitmap(sourcePath);
                using var dest   = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
                using var g      = Graphics.FromImage(dest);
                g.CompositingMode    = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode      = SmoothingMode.AntiAlias;
                g.PixelOffsetMode    = PixelOffsetMode.HighQuality;
                g.DrawImage(source, 0, 0, 128, 128);
                dest.Save(destPng, ImageFormat.Png);
            }
            catch
            {
                try { File.Copy(sourcePath, destPng, overwrite: true); } catch { }
            }
        }

        private static void CreateDefaultIcon(string destPng)
        {
            try
            {
                using var bmp = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
                using var g   = Graphics.FromImage(bmp);
                g.Clear(Color.FromArgb(255, 35, 54, 106));
                bmp.Save(destPng, ImageFormat.Png);
            }
            catch { }
        }

        // ── XML helpers ───────────────────────────────────────────────────────

        private static XmlElement AppendElem(XmlDocument doc, XmlNode parent, string name)
        {
            var e = doc.CreateElement(name);
            parent.AppendChild(e);
            return e;
        }

        // ── Misc helpers ──────────────────────────────────────────────────────

        private static string MakeUniqueName(string name, HashSet<string> used)
        {
            if (!used.Contains(name)) return name;
            string stem = Path.GetFileNameWithoutExtension(name);
            string ext  = Path.GetExtension(name);
            int n = 2;
            string candidate;
            do { candidate = $"{stem}_{n++}{ext}"; } while (used.Contains(candidate));
            return candidate;
        }

        private static string GetVersionPart(string version, int index)
        {
            if (string.IsNullOrWhiteSpace(version)) return "0";
            var parts = version.Split('.');
            return index < parts.Length && int.TryParse(parts[index].Trim(), out int v)
                ? v.ToString() : "0";
        }

        // OIV color format per spec: $AARRGGBB (hex, uppercase OK)
        private static string ToArgbHex(Color c)
            => $"${c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color ParseHexColor(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                    return Color.FromArgb(255,
                        Convert.ToInt32(hex[..2], 16),
                        Convert.ToInt32(hex[2..4], 16),
                        Convert.ToInt32(hex[4..6], 16));
            }
            catch { }
            return Color.FromArgb(255, 35, 54, 106);
        }

        // XmlTextWriter needs a TextWriter that reports UTF-8 encoding
        private sealed class StringWriterUtf8 : System.IO.StringWriter
        {
            private readonly StringBuilder _sb;
            public StringWriterUtf8(StringBuilder sb) : base(sb) { _sb = sb; }
            public override Encoding Encoding => new UTF8Encoding(false);
        }

        // Writes folders/files recursively.
        // currentArchiveGamePath:
        //   null  = we are not currently inside an <archive>
        //   value = full game path of the archive we are currently inside
        private static void WriteArchiveChildren(
            XmlDocument doc,
            XmlElement parent,
            OIVProject project,
            List<OIVFolder> folders,
            Dictionary<OIVFileEntry, string> resolved,
            Dictionary<string, string> sourceToContentName,
            string? currentArchiveGamePath = null)
        {
            foreach (var folder in folders)
            {
                var filesHere = project.Files.Where(f => f.FolderId == folder.Id).ToList();
                var children = project.Folders.Where(f => f.ParentId == folder.Id).ToList();

                if (folder.IsRpf)
                {
                    string fullArchivePath = BuildFullFolderPath(folder, project);

                    // If this is a top-level archive, use full path.
                    // If this archive is inside another archive, use relative path.
                    string archivePathForXml = currentArchiveGamePath == null
                        ? fullArchivePath
                        : MakeRelativePathInsideArchive(fullArchivePath, currentArchiveGamePath);

                    var arc = AppendElem(doc, parent, "archive");
                    arc.SetAttribute("path", archivePathForXml);
                    arc.SetAttribute("createIfNotExist", "True");
                    arc.SetAttribute("type", "RPF7");

                    // Files directly inside this archive (or subfolders under it) must be relative to THIS archive
                    foreach (var file in filesHere)
                    {
                        string contentName = sourceToContentName[file.SourcePath];
                        string fullPath = resolved[file];
                        string insideThisArchive = MakeRelativePathInsideArchive(fullPath, fullArchivePath);

                        var add = AppendElem(doc, arc, "add");
                        add.SetAttribute("source", contentName);
                        add.InnerText = insideThisArchive;
                    }

                    // Recurse into child folders, now inside this archive
                    WriteArchiveChildren(
                        doc,
                        arc,
                        project,
                        children,
                        resolved,
                        sourceToContentName,
                        fullArchivePath);
                }
                else
                {
                    // Non-RPF folder:
                    // - if we're not inside an archive, files use full install path
                    // - if we ARE inside an archive, files use path relative to the current archive
                    foreach (var file in filesHere)
                    {
                        string contentName = sourceToContentName[file.SourcePath];
                        string fullPath = resolved[file];

                        string xmlPath = currentArchiveGamePath == null
                            ? fullPath
                            : MakeRelativePathInsideArchive(fullPath, currentArchiveGamePath);

                        var add = AppendElem(doc, parent, "add");
                        add.SetAttribute("source", contentName);
                        add.InnerText = xmlPath;
                    }

                    // Child folders stay in same archive context
                    WriteArchiveChildren(
                        doc,
                        parent,
                        project,
                        children,
                        resolved,
                        sourceToContentName,
                        currentArchiveGamePath);
                }
            }
        }

        // Full folder path from root of target game
        private static string BuildFullFolderPath(OIVFolder target, OIVProject project)
        {
            var parts = new List<string> { target.Name };
            int? cur = target.ParentId;

            while (cur.HasValue)
            {
                var folder = project.Folders.Find(f => f.Id == cur.Value);
                if (folder == null) break;
                parts.Insert(0, folder.Name);
                cur = folder.ParentId;
            }

            return string.Join("\\", parts);
        }

        // Converts a full game path to a path relative to an archive root.
        // Example:
        //   fullPath     = update\update.rpf\common\data\dlclist.xml
        //   archivePath  = update\update.rpf
        //   result       = common\data\dlclist.xml
        private static string MakeRelativePathInsideArchive(string fullPath, string archivePath)
        {
            if (!fullPath.StartsWith(archivePath, StringComparison.OrdinalIgnoreCase))
                return fullPath;

            string relative = fullPath.Substring(archivePath.Length).TrimStart('\\');
            return relative;
        }
        private static string MakeFlatFolderName(string path)
        {
            path = path.Replace("\\", "/").Trim('/');

            foreach (char c in Path.GetInvalidFileNameChars())
                path = path.Replace(c, '-');

            return path.Replace("/", "-");
        }

        private static string MakeUniqueContentPath(string relativePath, HashSet<string> used)
        {
            relativePath = relativePath.Replace("/", "\\");

            if (!used.Contains(relativePath))
                return relativePath;

            string folder = Path.GetDirectoryName(relativePath) ?? "";
            string file = Path.GetFileNameWithoutExtension(relativePath);
            string ext = Path.GetExtension(relativePath);

            int i = 2;
            string candidate;

            do
            {
                candidate = Path.Combine(folder, $"{file}_{i}{ext}");
                i++;
            }
            while (used.Contains(candidate));

            return candidate;
        }

    }
}
