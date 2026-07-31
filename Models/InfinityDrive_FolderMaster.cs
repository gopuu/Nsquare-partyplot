namespace ERPAdmin.Models
{
    public class InfinityDriveFolderRequest
    {
        public long Id { get; set; }
    }

    public class InfinityDriveFolderSaveRequest : BaseClass
    {
        public long Id { get; set; }
        public string FolderName { get; set; } = "";
        public long ParentFolderId { get; set; }
        public int SortOrder { get; set; }
    }

    public class InfinityDriveFolderDeleteRequest : BaseClass
    {
        public long Id { get; set; }
    }

    public class InfinityDriveFolderModel
    {
        public long Id { get; set; }
        public string FolderName { get; set; } = "";
        public long ParentFolderId { get; set; }
        public string ParentFolderName { get; set; } = "";
        public int SortOrder { get; set; }
    }
}
