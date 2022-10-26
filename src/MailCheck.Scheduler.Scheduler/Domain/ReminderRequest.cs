using System;

namespace MailCheck.Scheduler.Scheduler.Domain
{
    public class ReminderRequest
    {
        public string Service { get; }

        public string ResourceId { get; }

        public DateTime ScheduledTime { get; }
        
        public ReminderRequest(string service, string resourceId, DateTime scheduledTime)
        {
            Service = service;
            ResourceId = resourceId;
            ScheduledTime = scheduledTime;
        }
    }
}