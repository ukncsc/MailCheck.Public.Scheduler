using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;

namespace MailCheck.Scheduler.QueueProcessor.Seeding
{
    internal interface ISeeder
    {
        Task SeedCreateScheduledReminder();
    }

    internal class Seeder : ISeeder
    {
        private readonly IDomainDao _domainDao;
        private readonly ISqsPublisher _publisher;
        private readonly ISeederConfig _config;

        public Seeder(IDomainDao domainDao, ISqsPublisher publisher, ISeederConfig config)
        {
            _domainDao = domainDao;
            _publisher = publisher;
            _config = config;
        }

        public async Task SeedCreateScheduledReminder()
        {
            List<string> domains = await _domainDao.GetDomains();

            List<CreateScheduledReminder> createScheduledReminders =
                domains.Select(domain => new CreateScheduledReminder(
                    Guid.NewGuid().ToString(),
                    _config.ServiceName,
                    domain,
                    default)).ToList();

            int count = 0;
            foreach (IEnumerable<CreateScheduledReminder> createScheduledReminder in createScheduledReminders.Batch(10))
            {
                List<Message> messages = createScheduledReminder.Cast<Message>().ToList();
                await _publisher.Publish(messages, _config.SnsTopicToSeedArn);
                Console.WriteLine($@"Processed {count += messages.Count} events.");
            }
        }
    }
}