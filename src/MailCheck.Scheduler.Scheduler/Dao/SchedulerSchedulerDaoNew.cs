using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MySql.Data.MySqlClient;
using MailCheck.Common.Data.Util;
using MailCheck.Common.Util;
using MailCheck.Scheduler.Scheduler.Config;
using MailCheck.Scheduler.Scheduler.Domain;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Scheduler.Scheduler.Dao
{
    public class SchedulerSchedulerDaoNew : ISchedulerSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly ISchedulerSchedulerConfig _config;
        private readonly IClock _clock;

        public SchedulerSchedulerDaoNew(IConnectionInfoAsync connectionInfo, ISchedulerSchedulerConfig config, IClock clock)
        {
            _connectionInfo = connectionInfo;
            _config = config;
            _clock = clock;
        }

        public async Task<List<ReminderRequest>> GetExpiredSchedulerReminders()
        {
            MySqlParameter[] parameters =
            {
                new MySqlParameter("currentDateTime", _clock.GetDateTimeUtc()),
                new MySqlParameter("limitNumber", _config.BatchSize)
            };

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            List<ReminderRequest> expiredReminders = new List<ReminderRequest>();
            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString,
                SchedulerSchedulerDaoResources.SelectSchedulerRecord, parameters))
            {
                while (await reader.ReadAsync())
                {
                    expiredReminders.Add(
                        new ReminderRequest(
                            reader.GetString("service"),
                            reader.GetString("resource_id"),
                            reader.GetDateTime("scheduled_time")));
                }
            }

            return expiredReminders;
        }

        public async Task RecordReminderSent(ReminderRequest reminderRequest)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            int maxInterval = GetSchedulerInterval(reminderRequest.Service);
            int minInterval = (int)Math.Floor(maxInterval * 0.75);

            DateTime scheduledTime = reminderRequest.ScheduledTime.AddSeconds(
                new Random().Next(minInterval, maxInterval));

            MySqlParameter[] parameters =
            {
                new MySqlParameter("service", reminderRequest.Service),
                new MySqlParameter("resource_id", reminderRequest.ResourceId),
                new MySqlParameter("scheduled_time", scheduledTime),
            };
            
            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SchedulerSchedulerDaoResources.UpdateSchedulerRecord, parameters);
        }

        private int GetSchedulerInterval(string serviceName)
        {
            if (_config.SchedulerIntervalOverrides.TryGetValue(serviceName, out int overrideInterval))
            {
                return overrideInterval;
            };

            return _config.DefaultSchedulerInterval;
        }
    }
}
