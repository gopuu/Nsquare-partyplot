namespace ERPAdmin.Models
{
    public class TaskCategoryMaster
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public int PriorityNo { get; set; } = 0;
    }


    public class GetTaskCategoryMasterRequest
    {
        public long Id { get; set; } // 0 = All
    }

    public class SaveTaskCategoryMasterRequest
    {
        public long Id { get; set; } // 0 = Insert, >0 = Update
        public string? CategoryName { get; set; }
        public int PriorityNo { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteTaskCategoryMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class TaskCategoryMasterModel
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public int PriorityNo { get; set; }
    }

}
