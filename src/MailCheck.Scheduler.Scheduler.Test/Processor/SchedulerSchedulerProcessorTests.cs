using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Scheduler.Scheduler.Config;
using MailCheck.Scheduler.Scheduler.Dao;
using MailCheck.Scheduler.Scheduler.Processor;
using MailCheck.Scheduler.Scheduler.Domain;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Linq;

namespace MailCheck.Scheduler.Scheduler.Processor
{
    [TestFixture]
    public class SchedulerSchedulerProcessorTests
    {
        private ISchedulerSchedulerDao _dao;
        private SchedulerSchedulerProcessor _schedulerSchedulerProcessor;
        private IMessagePublisher _publisher;
        private ISchedulerSchedulerConfig _config;
        private ILogger<SchedulerSchedulerProcessor> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ISchedulerSchedulerDao>();
            _publisher = A.Fake<IMessagePublisher>();
            _config = A.Fake<ISchedulerSchedulerConfig>();
            _log = A.Fake<ILogger<SchedulerSchedulerProcessor>>();
            _schedulerSchedulerProcessor = A.Fake<SchedulerSchedulerProcessor>();
            A.Fake<IProcess>();
            
            _schedulerSchedulerProcessor = new SchedulerSchedulerProcessor(
                _publisher,
                _config,
                _dao,
                _log
            );
        }

        [Test]
         public async Task ItShouldNotPublishOrDeleteWhenThereAreNoExpiredRecords()
        {
            ProcessResult result = await _schedulerSchedulerProcessor.Process();
            A.CallTo(() => _publisher.Publish(A<Message>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            Assert.That(result.ContinueProcessing, Is.False); 
        }

        [Test]
        public async Task ItShouldPublishSomethingWhenThereAreExpiredRecords()
        {
            A.CallTo(() => _config.PublisherConnectionString).Returns("testConnectionString");
            ReminderRequest reminderRequest = new ReminderRequest("serviceExample", "resourceIdExample", DateTime.Now);
            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).Returns(new List<ReminderRequest>{reminderRequest});
            
            ProcessResult result = await _schedulerSchedulerProcessor.Process();
            
            A.CallTo(() => _publisher.Publish(A<ScheduledReminder>.Ignored, "testConnectionString", "serviceExampleScheduledReminder")).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dao.RecordReminderSent(reminderRequest)).MustHaveHappenedOnceExactly();
            Assert.That(result.ContinueProcessing, Is.True);
        }

        [Test]
        public async Task ProcessWithFifteenMessageTest()
        {
            A.CallTo(() => _config.PublisherConnectionString).Returns("testConnectionString");
            ReminderRequest reminderRequest = new ReminderRequest("serviceExample", "resourceIdExample", DateTime.Now);
            List<ReminderRequest> reminders = Enumerable.Repeat(reminderRequest, 15).ToList();

            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).Returns(reminders);

            ProcessResult result = await _schedulerSchedulerProcessor.Process();

            Assert.That(result.ContinueProcessing, Is.True);

            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<ScheduledReminder>.That.Matches(x =>
                x.ResourceId == "resourceIdExample"), A<string>._, "serviceExampleScheduledReminder"))
                .MustHaveHappened(15, Times.Exactly);

            A.CallTo(() => _dao.RecordReminderSent(A<ReminderRequest>.That.Matches(x =>
                x.ResourceId == "resourceIdExample" && x.Service == "serviceExample")))
                .MustHaveHappened(15, Times.Exactly);
        }

        [Test]
        public async Task ProcessWithFifteenMessageAndOneFailedPublishTest()
        {
            A.CallTo(() => _config.PublisherConnectionString).Returns("testConnectionString");
            ReminderRequest reminderRequest = new ReminderRequest("serviceExample", "resourceIdExample", DateTime.Now);
            List<ReminderRequest> reminders = Enumerable.Repeat(reminderRequest, 15).ToList();

            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).Returns(reminders);

            A.CallTo(() => _publisher.Publish(A<ScheduledReminder>._, A<string>._, A<string>._))
                .Returns(Task.FromException<Exception>(new Exception("test"))).Once()
                .Then.Returns(Task.CompletedTask);

            ProcessResult result = await _schedulerSchedulerProcessor.Process();

            Assert.That(result.ContinueProcessing, Is.True);

            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<ScheduledReminder>.That.Matches(x =>
                x.ResourceId == "resourceIdExample"), A<string>._, "serviceExampleScheduledReminder"))
                .MustHaveHappened(15, Times.Exactly);

            A.CallTo(() => _dao.RecordReminderSent(A<ReminderRequest>.That.Matches(x =>
                x.ResourceId == "resourceIdExample" && x.Service == "serviceExample")))
                .MustHaveHappened(5, Times.Exactly);
        }

        [Test]
        public async Task ProcessWithFifteenMessageAndOneFailedSaveTest()
        {
            A.CallTo(() => _config.PublisherConnectionString).Returns("testConnectionString");
            ReminderRequest reminderRequest = new ReminderRequest("serviceExample", "resourceIdExample", DateTime.Now);
            List<ReminderRequest> reminders = Enumerable.Repeat(reminderRequest, 15).ToList();

            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).Returns(reminders);

            A.CallTo(() => _publisher.Publish(A<ScheduledReminder>._, A<string>._, A<string>._))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _dao.RecordReminderSent(A<ReminderRequest>._))
                .Returns(Task.FromException<Exception>(new Exception("test")));

            ProcessResult result = await _schedulerSchedulerProcessor.Process();

            Assert.That(result.ContinueProcessing, Is.False);

            A.CallTo(() => _dao.GetExpiredSchedulerReminders()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<ScheduledReminder>.That.Matches(x =>
                x.ResourceId == "resourceIdExample"), A<string>._, "serviceExampleScheduledReminder"))
                .MustHaveHappened(10, Times.Exactly);

            A.CallTo(() => _dao.RecordReminderSent(A<ReminderRequest>.That.Matches(x =>
                x.ResourceId == "resourceIdExample" && x.Service == "serviceExample")))
                .MustHaveHappened(10, Times.Exactly);
        }
    }
}