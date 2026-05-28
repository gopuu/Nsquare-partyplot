namespace ERPAdmin.Models
{
    #region Notification_TemplateParameterList
    public class Notification_TemplateParameterList
    {
        public string? TempName { get; set; }
        public string? TempName_Title { get; set; }
        public string? PTFlag { get; set; }
        public long EventId { get; set; }
        public bool IsSendMsg { get; set; }
    }
    public class SaveNotification_TemplateParameterList
    {
        public long EventId { get; set; }
        public bool? IsSendMsg { get; set; }
    }
    public class GetNotification_TemplateParameterList
    {
        public long? EventId { get; set; } = 0;
    }

    #endregion

    #region Notification EVENT REMINDER
    public class GetNotification_EventReminderSettingsList
    {
        public long EventId { get; set; }
    }

    public class Notification_EventReminderSettingsList
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public int? TriggerDays { get; set; }
        public string? TriggerTime { get; set; }
        public string? TriggerTime_Format { get; set; }
        public bool? IsEnabled { get; set; }
        public long? EventId { get; set; }
        public string? LastModifiedOn { get; set; }
    }

    public class SaveNotification_EventReminderSettings : BaseClass
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public int? TriggerDays { get; set; }
        public string? TriggerTime { get; set; }
        public bool? IsEnabled { get; set; }
        public long? EventId { get; set; }
    }
    #endregion

}
