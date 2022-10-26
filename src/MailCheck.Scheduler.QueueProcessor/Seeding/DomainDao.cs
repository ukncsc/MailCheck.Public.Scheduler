using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using MailCheck.Common.Data.Util;
using MailCheck.Common.Data.Abstractions;

namespace MailCheck.Scheduler.QueueProcessor.Seeding
{
    internal interface IDomainDao
    {
        Task<List<string>> GetDomains();
    }

    internal class DomainDao : IDomainDao
    {
        private readonly IConnectionInfo _connectionInfo;

        public DomainDao(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<string>> GetDomains()
        {
            List<string> list = new List<string>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString,
                @"SELECT d.name, d.created_date, u.email as created_by FROM domain d LEFT JOIN user u on u.id = d.created_by WHERE d.publish OR d.monitor;"))
            {
                while (reader.Read())
                {
                    list.Add(reader.GetString("name"));
                }
            }

            return list;
        }
    }
}