using Amazon.Runtime;
using Amazon.SQS;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Scheduler.QueueProcessor.Seeding
{
    internal class SeederFactory
    {
        public static ISeeder Create(string connectionString, string sqsQueueUrl, string serviceName)
        {
            return new ServiceCollection()
                .AddTransient<IConnectionInfo>(_ => new StringConnectionInfo(connectionString))
                .AddTransient<IDomainDao, DomainDao>()
                .AddTransient<IAmazonSQS>(_ => new AmazonSQSClient(new EnvironmentVariablesAWSCredentials()))
                .AddTransient<ISqsPublisher, SqsPublisher>()
                .AddTransient<ISeederConfig>(_ => new SeederConfig(sqsQueueUrl, serviceName))
                .AddTransient<ISeeder, Seeder>()
                .BuildServiceProvider()
                .GetRequiredService<ISeeder>();
        }
    }
}