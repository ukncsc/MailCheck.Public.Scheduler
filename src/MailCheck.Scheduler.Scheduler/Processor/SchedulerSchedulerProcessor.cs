using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Scheduler.Scheduler.Config;
using MailCheck.Scheduler.Scheduler.Dao;
using MailCheck.Scheduler.Scheduler.Domain;
using Microsoft.Extensions.Logging;

namespace MailCheck.Scheduler.Scheduler.Processor
{
    public class SchedulerSchedulerProcessor : IProcess
    {
        private static readonly TimeSpan PublishErrorDelay = TimeSpan.FromSeconds(5);

        private readonly IMessagePublisher _publisher;
        private readonly ILogger<SchedulerSchedulerProcessor> _log;
        private readonly ISchedulerSchedulerConfig _config;
        private readonly ISchedulerSchedulerDao _dao;

        public SchedulerSchedulerProcessor(
            IMessagePublisher publisher,
            ISchedulerSchedulerConfig config,
            ISchedulerSchedulerDao dao,
            ILogger<SchedulerSchedulerProcessor> log)
        {
            _publisher = publisher;
            _config = config;
            _dao = dao;
            _log = log;
        }

        public async Task<ProcessResult> Process()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<ReminderRequest> expiredReminders = await _dao.GetExpiredSchedulerReminders();

            int expiredRemindersCount = expiredReminders.Count;

            if (expiredRemindersCount == 0)
            {
                _log.LogInformation($"Found no expired records");
                return ProcessResult.Stop;
            }


            _log.LogInformation($"Found {expiredRemindersCount} expired reminders.");
            int totalBatches = 0;
            int successfulBatches = 0;

            foreach (var batchItems in expiredReminders.Batch(10))
            {
                totalBatches++;
                var batch = batchItems.ToArray();

                try
                {
                    var runningTasks = batch.Select(PublishReminder);
                    await Task.WhenAll(runningTasks);
                }
                catch (Exception e)
                {
                    // If we have a failure publishing we log it, pause for a bit and then continue to next batch
                    _log.LogError(e, $"Exception occurred publishing batch - skipping to next batch");
                    await Task.Delay(PublishErrorDelay);
                    continue;
                }

                try
                {
                    var runningTasks = batch.Select(RecordReminderSent);
                    await Task.WhenAll(runningTasks);
                }
                catch (Exception e)
                {
                    // If we have a failure saving to the database we log it and stop further processing
                    _log.LogError(e, "Exception occurred recording batch reminder sent - halting processing");
                    return ProcessResult.Stop;
                }
                
                _log.LogInformation($"Published and Updated batch of {batch.Length} reminders in {stopwatch.Elapsed}");
                successfulBatches++;
            }

            _log.LogInformation($"Successfully processing {successfulBatches} of {totalBatches} batches took: {stopwatch.Elapsed}");
            stopwatch.Stop();

            return ProcessResult.Continue;
        }

        private async Task PublishReminder(ReminderRequest reminderRequest)
        {
            ScheduledReminder scheduledReminder = new ScheduledReminder(Guid.NewGuid().ToString(), reminderRequest.ResourceId);
            string messageTypeOverride = reminderRequest.Service + "ScheduledReminder";

            try
            { 
                await _publisher.Publish(scheduledReminder, _config.PublisherConnectionString, messageTypeOverride);
            }
            catch (Exception cannotPublish)
            { 
                throw new Exception($"Exception occurred attempting to publish reminder for resourceId: {reminderRequest.ResourceId}," +
                    $"service: {reminderRequest.Service} and scheduledTime: {reminderRequest.ScheduledTime}", cannotPublish);
            }
        }

        private async Task RecordReminderSent(ReminderRequest reminderRequest)
        {
            try
            {
                await _dao.RecordReminderSent(reminderRequest);
            }
            catch (Exception cannotUpdateOrDelete)
            {
                throw new Exception($"Exception occurred attempting to update reminder for resourceId: {reminderRequest.ResourceId}," +
                    $"service {reminderRequest.Service} and scheduledTime {reminderRequest.ScheduledTime}", cannotUpdateOrDelete);
            }
        }
    }
}