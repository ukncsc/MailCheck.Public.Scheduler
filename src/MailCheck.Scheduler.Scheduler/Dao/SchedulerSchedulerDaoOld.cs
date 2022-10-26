using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MySql.Data.MySqlClient;
using MailCheck.Common.Data.Util;
using MailCheck.Scheduler.Scheduler.Config;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Scheduler.Scheduler.Domain;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Scheduler.Scheduler.Dao
{
    public class SchedulerSchedulerDaoOld : ISchedulerSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly ISchedulerSchedulerConfig _config;

        public SchedulerSchedulerDaoOld(IConnectionInfoAsync connectionInfo, ISchedulerSchedulerConfig config)
        {
            _connectionInfo = connectionInfo;
            _config = config;
        }

        public async Task<List<ReminderRequest>> GetExpiredSchedulerReminders()
        {
            MySqlParameter[] parameters =
            {
                new MySqlParameter("currentDateTime", DateTime.UtcNow),
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

            MySqlParameter[] parameters =
            {
                new MySqlParameter("service", reminderRequest.Service),
                new MySqlParameter("resource_id", reminderRequest.ResourceId),
                new MySqlParameter("scheduled_time", reminderRequest.ScheduledTime),
            };
            
            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SchedulerSchedulerDaoResources.DeleteSchedulerRecord, parameters);
        }
    }
}
