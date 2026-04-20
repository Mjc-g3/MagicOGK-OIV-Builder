using System.Collections.Generic;

namespace MagicOGK_OIV_Builder
{
    public class OIVFileEntry
    {
        public int    Id         { get; set; }
        public string FileName   { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string Type       { get; set; } = "content";
        // Which folder node this file lives under (null = unassigned root)
        public int? FolderId { get; set; } = null;
    }

    // Represents a named folder the user defines inside the OIV (e.g. a dlcpack folder)
    public class OIVFolder
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;  // folder name, e.g. "MyMod_dlc"
        // Full RPF path this folder lives inside, e.g. "mods\update\x64\dlcpacks"
        public string ArchivePath { get; set; } = string.Empty;
        // Whether to register this folder in dlclist.xml automatically on install
        public bool   AddToDlcList { get; set; } = false;
    }

    public class OIVProject
    {
        public string ModName     { get; set; } = string.Empty;
        public string Author      { get; set; } = string.Empty;
        public string Version     { get; set; } = "1.0";
        public string VersionTag  { get; set; } = "Stable";
        public string Description { get; set; } = string.Empty;
        // Hex RRGGBB for the OIV installer banner
        public string BannerColor { get; set; } = "640000";
        // Absolute path to the preview photo — gets copied as icon.png into the .oiv
        public string PhotoPath   { get; set; } = string.Empty;
        public int    NextId      { get; set; } = 1;
        public List<OIVFileEntry> Files   { get; set; } = new List<OIVFileEntry>();
        public List<OIVFolder>    Folders { get; set; } = new List<OIVFolder>();
    }
}
