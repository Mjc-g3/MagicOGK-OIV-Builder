using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace MagicOGK_OIV_Builder
{
    public static class OIVBuilder
    {
        public static void Build(OIVProject project, string outputPath)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "MagicOGK_Build_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                string contentDir = Path.Combine(tempDir, "content");
                Directory.CreateDirectory(contentDir);

                var processedFiles = new List<ProcessedFile>();

                foreach (var file in project.Files)
                {
                    if (!File.Exists(file.SourcePath)) continue;

                    string targetFolder = file.TargetPath.Trim().Replace('\\', '/');
                    if (targetFolder.Length > 0 && !targetFolder.EndsWith("/"))
                        targetFolder += "/";

                    string subDir = Path.Combine(contentDir, targetFolder.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(subDir);
                    File.Copy(file.SourcePath, Path.Combine(subDir, file.FileName), overwrite: true);

                    processedFiles.Add(new ProcessedFile
                    {
                        Entry = file,
                        ArchivePath = "content/" + targetFolder + file.FileName,
                        TargetFolder = targetFolder
                    });
                }

                string assemblyXml = GenerateAssemblyXml(project, processedFiles);
                File.WriteAllText(Path.Combine(tempDir, "assembly.xml"), assemblyXml, new UTF8Encoding(false));

                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                ZipFile.CreateFromDirectory(tempDir, outputPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        private class ProcessedFile
        {
            public OIVFileEntry Entry { get; set; } = null!;
            public string ArchivePath { get; set; } = string.Empty;
            public string TargetFolder { get; set; } = string.Empty;
        }

        private static string GenerateAssemblyXml(OIVProject project, List<ProcessedFile> files)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("package");
                writer.WriteAttributeString("version", "2.0");
                writer.WriteAttributeString("id", SanitizeId(project.ModName));

                WriteMetadata(writer, project);

                writer.WriteStartElement("content");

                var byTarget = new Dictionary<string, List<ProcessedFile>>();
                foreach (var pf in files)
                {
                    string key = pf.TargetFolder;
                    if (!byTarget.ContainsKey(key))
                        byTarget[key] = new List<ProcessedFile>();
                    byTarget[key].Add(pf);
                }

                foreach (var kvp in byTarget)
                {
                    string folderPath = kvp.Key.TrimEnd('/');

                    if (folderPath.Length == 0)
                    {
                        foreach (var pf in kvp.Value)
                            WriteFileElement(writer, pf);
                    }
                    else if (folderPath.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    {
                        writer.WriteStartElement("archive");
                        writer.WriteAttributeString("path", folderPath);
                        writer.WriteAttributeString("createIfNotExist", "True");
                        writer.WriteAttributeString("type", "RPF8");
                        foreach (var pf in kvp.Value)
                            WriteFileElement(writer, pf);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        string rpfPart = ExtractRpfAncestor(folderPath);
                        string innerPath = rpfPart.Length > 0
                            ? folderPath.Substring(rpfPart.Length).TrimStart('/')
                            : folderPath;

                        writer.WriteStartElement("archive");
                        writer.WriteAttributeString("path", rpfPart.Length > 0 ? rpfPart : folderPath);
                        writer.WriteAttributeString("createIfNotExist", "True");
                        writer.WriteAttributeString("type", "RPF8");

                        if (innerPath.Length > 0)
                        {
                            writer.WriteStartElement("archive");
                            writer.WriteAttributeString("path", innerPath);
                            writer.WriteAttributeString("createIfNotExist", "True");
                            writer.WriteAttributeString("type", "RPF8");
                            foreach (var pf in kvp.Value)
                                WriteFileElement(writer, pf);
                            writer.WriteEndElement();
                        }
                        else
                        {
                            foreach (var pf in kvp.Value)
                                WriteFileElement(writer, pf);
                        }

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static void WriteMetadata(XmlWriter writer, OIVProject project)
        {
            writer.WriteStartElement("metadata");

            writer.WriteStartElement("name");
            writer.WriteString(project.ModName);
            writer.WriteEndElement();

            writer.WriteStartElement("version");
            writer.WriteStartElement("major");
            writer.WriteString(GetVersionPart(project.Version, 0));
            writer.WriteEndElement();
            writer.WriteStartElement("minor");
            writer.WriteString(GetVersionPart(project.Version, 1));
            writer.WriteEndElement();
            writer.WriteStartElement("tag");
            writer.WriteString(project.VersionTag.ToLower());
            writer.WriteEndElement();
            writer.WriteEndElement();

            if (!string.IsNullOrWhiteSpace(project.Author))
            {
                writer.WriteStartElement("author");
                writer.WriteString(project.Author);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(project.Description))
            {
                writer.WriteStartElement("description");
                writer.WriteCData(project.Description);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void WriteFileElement(XmlWriter writer, ProcessedFile pf)
        {
            if (pf.Entry.Type == "xmledit")
            {
                writer.WriteStartElement("xmlEdit");
                writer.WriteAttributeString("source", pf.ArchivePath);
                writer.WriteString(pf.Entry.FileName);
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteStartElement("add");
                writer.WriteAttributeString("source", pf.ArchivePath);
                writer.WriteString(pf.Entry.FileName);
                writer.WriteEndElement();
            }
        }

        private static string ExtractRpfAncestor(string path)
        {
            string[] parts = path.Split('/');
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                if (sb.Length > 0) sb.Append('/');
                sb.Append(part);
                if (part.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    return sb.ToString();
            }
            return string.Empty;
        }

        private static string SanitizeId(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "mod";
            var sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                    sb.Append(c);
                else if (c == ' ')
                    sb.Append('-');
            }
            return sb.Length > 0 ? sb.ToString().ToLower() : "mod";
        }

        private static string GetVersionPart(string version, int index)
        {
            if (string.IsNullOrWhiteSpace(version)) return "0";
            var parts = version.Split('.');
            if (index < parts.Length && int.TryParse(parts[index], out int val))
                return val.ToString();
            return "0";
        }
    }
}
