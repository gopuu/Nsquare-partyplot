namespace ERPAdmin.Models
{
    //SOP Rights
    public class InfinityGrowth_SOPEmployeeAccessRequest
    {
        public long EmployeeId { get; set; }
    }
    public class InfinityGrowth_SOPEmployeeAccessRightModel
    {
        public long DocumentId { get; set; }
        public string DocumentName { get; set; } = "";
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public long ParentCategoryId { get; set; }
        public long EmployeeId { get; set; }
        public long AccessRightId { get; set; }
        public bool CanView { get; set; }
    }
    public class SaveInfinityGrowth_SOPRightsRequest
    {
        public long StaffId { get; set; }
        public List<SaveInfinityGrowth_SOPEmployeeAccessRightModel> SOPRights { get; set; } 
            = new List<SaveInfinityGrowth_SOPEmployeeAccessRightModel>();
    }

    public class SaveInfinityGrowth_SOPEmployeeAccessRightModel
    {
        public long EmployeeId { get; set; }
        public long DocumentId { get; set; }
        public bool CanView { get; set; }
    }


    //Use Rights
    public class GetUserRightsRequest
    {
        public long StaffId { get; set; }
        public string? Ex1 { get; set; }
        public string? Ex2 { get; set; }
        public string? Ex3 { get; set; }
    }
   
    public class ModuleRights
    {
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Link { get; set; }
        public long StaffId { get; set; }
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
