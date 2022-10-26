using System;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Scheduler.QueueProcessor.Config;
using MailCheck.Scheduler.QueueProcessor.Dao;
using MailCheck.Scheduler.QueueProcessor.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Scheduler.QueueProcessor.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>() 
                .AddTransient<ISchedulerDao, SchedulerDao>()
                .AddTransient<ISchedulerQueueProcessorConfig, SchedulerQueueProcessorConfig>()
                .AddTransient<IClock, Clock>()
                .AddTransient<SchedulerQueueProcessor>();
        }
    }
}