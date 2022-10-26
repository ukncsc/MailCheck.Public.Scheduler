using System;
using System.Collections.Generic;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Scheduler.Scheduler.Domain;
using Newtonsoft.Json;

namespace MailCheck.Scheduler.Scheduler.Config
{
    public interface ISchedulerSchedulerConfig
    {
        string PublisherConnectionString { get; }
        int BatchSize { get; }
        int DefaultSchedulerInterval { get; }
        Dictionary<string, int> SchedulerIntervalOverrides { get; }
    }
    
    public class SchedulerSchedulerConfig : ISchedulerSchedulerConfig
    {
        public SchedulerSchedulerConfig(IEnvironmentVariables environmentVariables)
        {
            PublisherConnectionString = environmentVariables.Get("SnsTopicArn");
            BatchSize = environmentVariables.GetAsInt("BatchSize");
            DefaultSchedulerInterval = environmentVariables.GetAsInt("DefaultSchedulerInterval");
            SchedulerIntervalOverrides = JsonConvert.DeserializeObject<Dictionary<string, int>>(environmentVariables.Get("SchedulerIntervalOverrides") ?? "{}");
        }
        
        public string PublisherConnectionString { get; }
        public int BatchSize { get; }
        public int DefaultSchedulerInterval { get; }
        public Dictionary<string, int> SchedulerIntervalOverrides { get; }
    }
}
