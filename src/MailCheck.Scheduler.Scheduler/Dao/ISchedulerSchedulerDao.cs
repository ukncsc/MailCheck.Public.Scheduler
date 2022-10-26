using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Scheduler.Scheduler.Domain;

namespace MailCheck.Scheduler.Scheduler.Dao
{
    public interface ISchedulerSchedulerDao
    {
        Task<List<ReminderRequest>> GetExpiredSchedulerReminders();
        Task RecordReminderSent(ReminderRequest reminderRequest);
    }
}