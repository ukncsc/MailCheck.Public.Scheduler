using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Util;
using MailCheck.Scheduler.QueueProcessor.Config;
using MailCheck.Scheduler.QueueProcessor.Dao;
using Microsoft.Extensions.Logging;

namespace MailCheck.Scheduler.QueueProcessor.Processors
{
    public class SchedulerQueueProcessor:
        IHandle<CreateScheduledReminder>,
        IHandle<DeleteScheduledReminder>,
        IHandle<ReminderSuccessful>
    {
        private readonly ISchedulerDao _dao;
        private readonly ILogger<SchedulerQueueProcessor> _log;
        private readonly ISchedulerQueueProcessorConfig _config;
        private readonly IClock _clock;

        public SchedulerQueueProcessor(ISchedulerDao dao,
            ILogger<SchedulerQueueProcessor> log, ISchedulerQueueProcessorConfig config, IClock clock)
        {
            _dao = dao;
            _log = log;
            _config = config;
            _clock = clock;
        }

        public async Task Handle(CreateScheduledReminder message)
        {
            string service = message.Service;
            string resourceId = message.ResourceId;
            DateTime scheduledTime = message.ScheduledTime == default 
                ? GetInitialScheduledTime() : message.ScheduledTime;
            bool saved = await _dao.Save(service, resourceId, scheduledTime);

            if (saved)
            {
                _log.LogInformation("Scheduled message for Service: {service}, ResourceId: {resourceId} and " +
                                    "ScheduledTime: {scheduledTime} has been been inserted into DB.", service, resourceId, scheduledTime);
            }
            else
            {
                _log.LogWarning("Failed to insert Scheduled message for Service: {service}, ResourceId: {resourceId} " +
                                    "and ScheduledTime: {scheduledTime}. This could be because a schedule for this resource and service already exists", service, resourceId, scheduledTime) ;
            }
        }

        public async Task Handle(DeleteScheduledReminder message)
        {
            string service = message.Service;
            string resourceId = message.ResourceId;
            bool deleted = await _dao.Delete(service, resourceId);

            if (deleted)
            {
                _log.LogInformation("Schedule record for Service: {service} and ResourceId: {resourceId} " +
                                    "has been been deleted from the DB.", service, resourceId);
            }
            else
            {
                _log.LogWarning("Failed to delete Schedule record for Service: {service} and ResourceId: {resourceId}.",
                              service, resourceId);
            }
        }

        public async Task Handle(ReminderSuccessful message)
        {
            string service = message.Service;
            string resourceId = message.ResourceId;
            DateTime lastSuccessful = message.PollTime;
            bool updated = await _dao.UpdateLastSuccessful(service, resourceId, lastSuccessful);

            if (updated)
            {
                _log.LogInformation("Schedule record for Service: {service} and ResourceId: {resourceId} " +
                                    "has been been updated with last successful time of {pollTime}.", 
                    service, resourceId, lastSuccessful);
            }
            else
            {
                _log.LogWarning($"Failed to update Schedule record last successful time as {lastSuccessful} for Service: {service} and ResourceId: {resourceId}.",
                    lastSuccessful, service, resourceId);
            }
        }

        private DateTime GetInitialScheduledTime()
        {
            return _clock.GetDateTimeUtc().AddSeconds(new Random().Next(0, _config.InitialInterval));
        }
    }
}