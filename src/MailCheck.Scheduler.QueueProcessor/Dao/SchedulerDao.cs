using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Scheduler.QueueProcessor.Dao
{
    public interface ISchedulerDao
    {
        Task<bool> Save(string service, string resourceId, DateTime scheduledTime);
        Task<bool> Delete(string service, string resourceId);
        Task<bool> UpdateLastSuccessful(string service, string resourceId, DateTime lastSuccessful);
    }
    
    public class SchedulerDao : ISchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public SchedulerDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<bool> Save(string service, string resourceId, DateTime scheduledTime)
        {
            MySqlParameter servParam = new MySqlParameter("service", service);
            MySqlParameter resParam = new MySqlParameter("resource_id", resourceId);
            MySqlParameter schParam = new MySqlParameter("scheduled_time", scheduledTime);
            
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SchedulerDaoResources.InsertScheduledReminder, servParam, resParam, schParam);

            return rowsAffected > 0;
        }
        
        public async Task<bool> Delete(string service, string resourceId)
        {
            MySqlParameter servParam = new MySqlParameter("service", service);
            MySqlParameter resParam = new MySqlParameter("resource_id", resourceId);
            
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SchedulerDaoResources.DeleteScheduledReminder, servParam, resParam);

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastSuccessful(string service, string resourceId, DateTime lastSuccessful)
        {
            MySqlParameter servParam = new MySqlParameter("service", service);
            MySqlParameter resParam = new MySqlParameter("resource_id", resourceId);
            MySqlParameter timeParam = new MySqlParameter("last_successful", lastSuccessful);
            
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SchedulerDaoResources.UpdateLastSuccessful, servParam, resParam, timeParam);

            return rowsAffected > 0;
        }
    }
}
