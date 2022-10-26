namespace MailCheck.Scheduler.QueueProcessor.Seeding
{
    internal interface ISeederConfig
    {
        string SnsTopicToSeedArn { get; }
        string ServiceName { get; }
    }

    internal class SeederConfig : ISeederConfig
    {
        public SeederConfig(string queueToSeedUrl, string serviceName)
        {
            SnsTopicToSeedArn = queueToSeedUrl;
            ServiceName = serviceName;
        }

        public string SnsTopicToSeedArn { get; }
        public string ServiceName { get; }
    }
}