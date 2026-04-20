using System;
using System.Collections.Generic;
using System.Drawing;
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
        // ─── Public entry point ───────────────────────────────────────────────

        public static void Build(OIVProject project, string outputPath)
        {
            foreach (var file in project.Files)
                if (!File.Exists(file.SourcePath))
                    throw new FileNotFoundException($"Source file not found: {file.SourcePath}");

            string tempDir = Path.Combine(Path.GetTempPath(), "MagicOGK_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // ── content/ folder ──────────────────────────────────────────
                string contentDir = Path.Combine(tempDir, "content");
                Directory.CreateDirectory(contentDir);

                var usedNames  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var packEntries = new List<PackEntry>();

                foreach (var file in project.Files)
                {
                    string safeName = MakeSafeContentName(file.FileName, usedNames);
                    usedNames.Add(safeName);
                    File.Copy(file.SourcePath, Path.Combine(contentDir, safeName), overwrite: true);

                    string resolvedTarget = ResolveTargetPath(file, project);

                    packEntries.Add(new PackEntry
                    {
                        ContentName = safeName,
                        FileName    = file.FileName,
                        TargetPath  = resolvedTarget.Replace('/', '\\').TrimStart('\\'),
                        Type        = file.Type
                    });
                }

                // ── dlclist patch XMLs ────────────────────────────────────────
                WriteDlcListPatchFiles(project, contentDir);

                // ── icon.png ─────────────────────────────────────────────────
                // OIV spec: icon.png must sit in the package root next to assembly.xml
                if (!string.IsNullOrWhiteSpace(project.PhotoPath) && File.Exists(project.PhotoPath))
                    ConvertToIcon(project.PhotoPath, Path.Combine(tempDir, "icon.png"));

                // ── assembly.xml ─────────────────────────────────────────────
                string xml = BuildAssemblyXml(project, packEntries);
                File.WriteAllText(Path.Combine(tempDir, "assembly.xml"), xml, new UTF8Encoding(false));

                // ── zip → .oiv ───────────────────────────────────────────────
                if (File.Exists(outputPath)) File.Delete(outputPath);
                ZipFile.CreateFromDirectory(tempDir, outputPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        // ─── Target path resolution ───────────────────────────────────────────
        // If a file is assigned to a folder node, prepend that folder's archive path + folder name.
        // Otherwise use the file's TargetPath as-is (user typed the full path manually).

        private static string ResolveTargetPath(OIVFileEntry file, OIVProject project)
        {
            if (file.FolderId.HasValue)
            {
                var folder = project.Folders.Find(f => f.Id == file.FolderId.Value);
                if (folder != null)
                {
                    string basePath = folder.ArchivePath.TrimEnd('\\') + "\\" + folder.Name;
                    string inner    = file.TargetPath.Trim().TrimStart('\\');
                    return string.IsNullOrWhiteSpace(inner) ? basePath : basePath + "\\" + inner;
                }
            }
            return file.TargetPath.Trim();
        }

        // ─── assembly.xml ─────────────────────────────────────────────────────

        private static string BuildAssemblyXml(OIVProject project, List<PackEntry> entries)
        {
            var ms = new MemoryStream();
            using (var w = XmlWriter.Create(ms, new XmlWriterSettings
            {
                Indent             = true,
                IndentChars        = "\t",
                Encoding           = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            }))
            {
                w.WriteStartDocument();
                w.WriteStartElement("package");
                w.WriteAttributeString("version", "2.1");
                w.WriteAttributeString("id", "{" + Guid.NewGuid().ToString().ToUpper() + "}");
                w.WriteAttributeString("target", "Five");

                WriteMetadata(w, project);
                WriteColors(w, project);
                WriteContent(w, project, entries);

                w.WriteEndElement();
                w.WriteEndDocument();
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // ─── <metadata> ───────────────────────────────────────────────────────

        private static void WriteMetadata(XmlWriter w, OIVProject project)
        {
            w.WriteStartElement("metadata");
            w.WriteElementString("name", project.ModName);

            w.WriteStartElement("version");
            w.WriteElementString("major", GetVersionPart(project.Version, 0));
            w.WriteElementString("minor", GetVersionPart(project.Version, 1));
            if (!string.IsNullOrWhiteSpace(project.VersionTag))
                w.WriteElementString("tag", project.VersionTag.ToUpper());
            w.WriteEndElement();

            w.WriteStartElement("author");
            w.WriteElementString("displayName", project.Author);
            w.WriteEndElement();

            w.WriteStartElement("description");
            w.WriteCData(string.IsNullOrWhiteSpace(project.Description)
                ? $"{project.ModName} by {project.Author}"
                : project.Description);
            w.WriteEndElement();

            w.WriteEndElement();
        }

        // ─── <colors> ─────────────────────────────────────────────────────────

        private static void WriteColors(XmlWriter w, OIVProject project)
        {
            Color header = string.IsNullOrWhiteSpace(project.BannerColor)
                ? Color.FromArgb(100, 0, 0)
                : ParseHexColor(project.BannerColor);

            w.WriteStartElement("colors");

            w.WriteStartElement("headerBackground");
            w.WriteAttributeString("useBlackTextColor", "False");
            w.WriteString(ToOivColor(header));
            w.WriteEndElement();

            w.WriteElementString("iconBackground", ToOivColor(Color.FromArgb(40, 0, 0)));

            w.WriteEndElement();
        }

        // ─── <content> ────────────────────────────────────────────────────────

        private static void WriteContent(XmlWriter w, OIVProject project, List<PackEntry> entries)
        {
            w.WriteStartElement("content");

            // Group entries by the RPF archive segment in their target path
            var rpfGroups  = new Dictionary<string, List<PackEntry>>(StringComparer.OrdinalIgnoreCase);
            var looseFiles = new List<PackEntry>();

            foreach (var e in entries)
            {
                string rpf = ExtractRpfPath(e.TargetPath);
                if (rpf.Length == 0) { looseFiles.Add(e); continue; }

                string inner = e.TargetPath.Substring(rpf.Length).TrimStart('\\');
                if (!rpfGroups.ContainsKey(rpf)) rpfGroups[rpf] = new List<PackEntry>();
                rpfGroups[rpf].Add(new PackEntry
                {
                    ContentName = e.ContentName,
                    FileName    = e.FileName,
                    TargetPath  = inner,
                    Type        = e.Type
                });
            }

            // RPF archive blocks
            foreach (var kv in rpfGroups)
            {
                w.WriteStartElement("archive");
                w.WriteAttributeString("path", kv.Key);
                w.WriteAttributeString("createIfNotExist", "True");
                w.WriteAttributeString("type", "RPF8");
                foreach (var e in kv.Value) WriteAddOrXml(w, e);
                w.WriteEndElement();
            }

            // Loose files (scripts/, plugins/, etc.)
            foreach (var e in looseFiles) WriteAddOrXml(w, e);

            // ── dlclist.xml injection ──────────────────────────────────────
            // For each folder with AddToDlcList = true, we already wrote a patch
            // XML into content/__dlclist_{id}.xml. Now reference it here so OIV
            // merges those entries into the game's dlclist.xml.
            var dlcFolders = project.Folders.Where(f => f.AddToDlcList).ToList();
            if (dlcFolders.Count > 0)
            {
                w.WriteStartElement("archive");
                w.WriteAttributeString("path", @"update\update.rpf");
                w.WriteAttributeString("createIfNotExist", "False");
                w.WriteAttributeString("type", "RPF8");

                w.WriteStartElement("archive");
                w.WriteAttributeString("path", @"common\data");
                w.WriteAttributeString("createIfNotExist", "False");
                w.WriteAttributeString("type", "RPF8");

                foreach (var folder in dlcFolders)
                {
                    w.WriteStartElement("xml");
                    w.WriteAttributeString("source", $"content/__dlclist_{folder.Id}.xml");
                    w.WriteString("dlclist.xml");
                    w.WriteEndElement();
                }

                w.WriteEndElement(); // common\data
                w.WriteEndElement(); // update\update.rpf
            }

            w.WriteEndElement(); // content
        }

        private static void WriteAddOrXml(XmlWriter w, PackEntry e)
        {
            string tag    = e.Type == "xmledit" ? "xml" : "add";
            string target = string.IsNullOrWhiteSpace(e.TargetPath)
                ? e.FileName
                : e.TargetPath.TrimEnd('\\') + "\\" + e.FileName;

            w.WriteStartElement(tag);
            w.WriteAttributeString("source", "content/" + e.ContentName);
            w.WriteString(target.TrimStart('\\'));
            w.WriteEndElement();
        }

        // ─── DLC patch XML files ──────────────────────────────────────────────
        // Generates one OIV xml-edit patch per dlclist folder.
        // OIV merges these into the game's dlclist.xml at install time.

        private static void WriteDlcListPatchFiles(OIVProject project, string contentDir)
        {
            foreach (var folder in project.Folders.Where(f => f.AddToDlcList))
            {
                string dlcEntry  = $"dlcpacks:/{folder.Name}/";
                string patchPath = Path.Combine(contentDir, $"__dlclist_{folder.Id}.xml");

                // Standard GTA5 dlclist.xml patch format that OpenIV merges via xpath
                string patchXml =
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<SMandatoryPacksData>
	<Paths>
		<Item>{dlcEntry}</Item>
	</Paths>
</SMandatoryPacksData>";
                File.WriteAllText(patchPath, patchXml, new UTF8Encoding(false));
            }
        }

        // ─── Icon ─────────────────────────────────────────────────────────────
        // Convert any image to PNG and save as icon.png in the package root.

        private static void ConvertToIcon(string sourcePath, string destPng)
        {
            try
            {
                using var img = Image.FromFile(sourcePath);
                using var bmp = new Bitmap(img, 512, 512);
                bmp.Save(destPng, ImageFormat.Png);
            }
            catch
            {
                // Best-effort fallback: just copy the file raw
                File.Copy(sourcePath, destPng, overwrite: true);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        // Finds the deepest .rpf segment in a backslash-separated path.
        private static string ExtractRpfPath(string target)
        {
            string[] parts = target.Split('\\');
            var sb   = new StringBuilder();
            string last = string.Empty;
            foreach (string p in parts)
            {
                if (sb.Length > 0) sb.Append('\\');
                sb.Append(p);
                if (p.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    last = sb.ToString();
            }
            return last;
        }

        private static string MakeSafeContentName(string fileName, HashSet<string> used)
        {
            if (!used.Contains(fileName)) return fileName;
            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext  = Path.GetExtension(fileName);
            int n = 2;
            string candidate;
            do { candidate = $"{name}_{n++}{ext}"; } while (used.Contains(candidate));
            return candidate;
        }

        private static string GetVersionPart(string version, int index)
        {
            if (string.IsNullOrWhiteSpace(version)) return "0";
            string[] parts = version.Split('.');
            return (index < parts.Length && int.TryParse(parts[index].Trim(), out int v))
                ? v.ToString() : "0";
        }

        private static string ToOivColor(Color c) => $"$FF{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color ParseHexColor(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                    return Color.FromArgb(255,
                        Convert.ToInt32(hex.Substring(0, 2), 16),
                        Convert.ToInt32(hex.Substring(2, 2), 16),
                        Convert.ToInt32(hex.Substring(4, 2), 16));
            }
            catch { }
            return Color.FromArgb(100, 0, 0);
        }

        private class PackEntry
        {
            public string ContentName { get; set; } = string.Empty;
            public string FileName    { get; set; } = string.Empty;
            public string TargetPath  { get; set; } = string.Empty;
            public string Type        { get; set; } = "content";
        }
    }
}
