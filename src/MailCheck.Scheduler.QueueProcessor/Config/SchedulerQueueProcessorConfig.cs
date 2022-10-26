using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Scheduler.QueueProcessor.Config
{
    public interface ISchedulerQueueProcessorConfig
    {
        string SnsTopicArn { get; }
        int InitialInterval { get; }
    }
    
    public class SchedulerQueueProcessorConfig : ISchedulerQueueProcessorConfig
    {
        public SchedulerQueueProcessorConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            InitialInterval = environmentVariables.GetAsInt("InitialInterval");
        }
        
        public string SnsTopicArn { get; }
        public int InitialInterval { get; }
    }
}