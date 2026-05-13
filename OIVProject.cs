using System.Collections.Generic;

namespace MagicOGK_OIV_Builder
{
    public class OIVFileEntry
    {
        public int    Id         { get; set; }
        public string FileName   { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        // SubPath is the path inside the parent folder (usually empty — file sits directly in folder)
        public string SubPath    { get; set; } = string.Empty;
        public string Type       { get; set; } = "content";  // content | replace | xmledit
        // Which OIVFolder node this file is placed under (null = unassigned / loose)
        public int?   FolderId   { get; set; } = null;

        // Legacy compat — kept so old .mogk files deserialise cleanly
        public string TargetPath { get => SubPath; set => SubPath = value; }

    }

    // A node in the virtual folder tree inside the .oiv.
    // The tree is: Root → any number of nested OIVFolders → files at the leaves.
    public class OIVFolder
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;
        // null = direct child of Root
        public int?   ParentId    { get; set; } = null;
        // True if this node is an RPF archive (shown with a different icon, affects assembly.xml)
        public bool   IsRpf       { get; set; } = false;
        // Whether to register this folder in dlclist.xml automatically on install.
        // Only meaningful on leaf folders that represent DLC packs.
        public bool   AddToDlcList { get; set; } = false;
    }

    public class OIVProject
    {
        public string ModName     { get; set; } = string.Empty;
        public string Author      { get; set; } = string.Empty;
        public string Website { get; set; } = "";
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
