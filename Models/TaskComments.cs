namespace ERPAdmin.Models
{
    public class TaskComments
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public string? CommentText { get; set; }
        public long CommentedBy { get; set; }
        public string? CommentedAt { get; set; }
    }

    public class GetTaskCommentsRequest
    {
        public long TaskId { get; set; }   // 0 = All
    }

    public class SaveTaskCommentsRequest
    {
        public long Id { get; set; }       // 0 = Insert, >0 = Update
        public long TaskId { get; set; }
        public string? CommentText { get; set; }
        public long CommentedBy { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteTaskCommentsRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class TaskCommentsModel
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public string? CommentText { get; set; }
        public long CommentedBy { get; set; }
        public string? CommentedAt { get; set; }
    }

}
