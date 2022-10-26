using System;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Scheduler.QueueProcessor.Dao;
using MailCheck.Scheduler.QueueProcessor.Processors;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FakeItEasy;
using MailCheck.Common.Util;
using MailCheck.Scheduler.QueueProcessor.Config;

namespace MailCheck.Scheduler.QueueProcessor.Test.Processors
{
    [TestFixture]
    public class SchedulerQueueProcessorTest
    {
        private const string Id = "34324";
        private const string ResourceId = "myResourceId";
        private const string Service = "DMARC";
        
        private SchedulerQueueProcessor _schedulerQueueProcessor;

        private ISchedulerDao _dao;
        private ILogger<SchedulerQueueProcessor> _log;
        private ISchedulerQueueProcessorConfig _config;
        private IClock _clock;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ISchedulerDao>();
            _log = A.Fake<ILogger<SchedulerQueueProcessor>>();
            _config = A.Fake<ISchedulerQueueProcessorConfig>();
            _clock = A.Fake<IClock>();
            _schedulerQueueProcessor = new SchedulerQueueProcessor(_dao, _log, _config, _clock);
        }

        [Test]
        public async Task ShouldHandleCreateScheduledReminderWithScheduledTime()
        {
            var message = new CreateScheduledReminder(Id, Service, ResourceId, DateTime.Now);
            
            await _schedulerQueueProcessor.Handle(message);

            A.CallTo(() => _dao.Save(message.Service, message.ResourceId, message.ScheduledTime)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task ShouldHandleCreateScheduledReminderWithDefaultTime()
        {
            DateTime timeNow = DateTime.Now;
            A.CallTo(() => _config.InitialInterval).Returns(1);
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(timeNow);
            
            var message = new CreateScheduledReminder(Id, Service, ResourceId, default);
            
            await _schedulerQueueProcessor.Handle(message);

            A.CallTo(() => _dao.Save(message.Service, message.ResourceId, A<DateTime>.That.Matches(_ => 
                _.Equals(timeNow.AddSeconds(0)) || _.Equals(timeNow.AddSeconds(1))))).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task ShouldHandleDeleteScheduledReminder()
        {
            var message = new DeleteScheduledReminder(Id, Service, ResourceId);
            
            await _schedulerQueueProcessor.Handle(message);

            A.CallTo(() => _dao.Delete(message.Service, message.ResourceId)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task ShouldHandleReminderSuccessful()
        {
            var message = new ReminderSuccessful(Id, Service, ResourceId, DateTime.Now);
            
            await _schedulerQueueProcessor.Handle(message);

            A.CallTo(() => _dao.UpdateLastSuccessful(message.Service, message.ResourceId, message.PollTime)).MustHaveHappenedOnceExactly();
        }
    }
}