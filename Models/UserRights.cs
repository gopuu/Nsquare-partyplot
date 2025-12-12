namespace ERPAdmin.Models
{
    public class GetUserRightsRequest
    {
        public long StaffId { get; set; }
    }
    public class ModuleRights
    {
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Link { get; set; }
        public long EmployeeId { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanView { get; set; }

        public bool IsAddNeeded { get; set; }
        public bool IsEditNeeded { get; set; }
        public bool IsDeleteNeeded { get; set; }
        public bool IsViewNeeded { get; set; }
    }
    public class UserRightsGroupModel
    {
        public long GroupId { get; set; }
        public string GroupTitle { get; set; }
        public List<ModuleRights> UserRights { get; set; }
    }
    
    //SAVE
    public class SaveUserRightsModel
    {
        public List<SaveModuleRights>? UserRights { get; set; }
    }
    public class SaveModuleRights
    {
        public long StaffId { get; set; }
        public long ModuleId { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanView { get; set; }
    }

}
