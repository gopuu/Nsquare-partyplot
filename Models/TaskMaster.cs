namespace ERPAdmin.Models
{
    public class GetPriorityLevelsRequest
    {
        public long Id { get; set; }  // Optional; 0 = fetch all
    }

    public class PriorityLevelModel
    {
        public long Id { get; set; }
        public int PriorityLevel { get; set; }     // 1 = High, 2 = Medium, 3 = Low
        public string? PriorityName { get; set; }
    }

    public class TaskMaster
    {
        public long Id { get; set; }
        public string? TaskTitle { get; set; }
        public string? TaskDescription { get; set; }

        public long? CategoryId { get; set; }
        public long? AssignedTo { get; set; }
        public long? AssignedBy { get; set; }

        public int? PriorityLevel { get; set; }

        public string? StartDate { get; set; }
        public string? DueDate { get; set; }
        public string? CompletedDate { get; set; }

        public long? StatusId { get; set; }
    }
    public class GetTaskMasterRequest
    {
        public long Id { get; set; }          // 0 = All
        public long AssignedTo { get; set; }  // Filter
        public long CategoryId { get; set; }  // Filter
        public long StatusId { get; set; }    // Filter
    }

    public class SaveTaskMasterRequest
    {
        public long Id { get; set; } // 0 = Insert, >0 = Update
        public string? TaskTitle { get; set; }
        public string? TaskDescription { get; set; }

        public long? CategoryId { get; set; }
        public long? AssignedTo { get; set; }
        public long? AssignedBy { get; set; }

        public int? PriorityLevel { get; set; } // 1=High,2=Medium,3=Low

        public string? StartDate { get; set; }
        public string? DueDate { get; set; }
        public string? CompletedDate { get; set; }

        public long? StatusId { get; set; }

        public long ActionBy { get; set; }
    }

    public class DeleteTaskMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class TaskMasterModel
    {
        public long Id { get; set; }
        public string? TaskTitle { get; set; }
        public string? TaskDescription { get; set; }

        public long? CategoryId { get; set; }
        public long? AssignedTo { get; set; }
        public long? AssignedBy { get; set; }

        public int? PriorityLevel { get; set; }

        public string? StartDate { get; set; }
        public string? DueDate { get; set; }
        public string? CompletedDate { get; set; }

        public long? StatusId { get; set; }
    }

}
