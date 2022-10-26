using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Scheduler.Scheduler.Config;
using MailCheck.Scheduler.Scheduler.Dao;
using MailCheck.Scheduler.Scheduler.Processor;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Scheduler.Scheduler.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                serializerSetting.Converters.Add(new StringEnumConverter());

                return serializerSetting;
            };

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<ISchedulerSchedulerConfig, SchedulerSchedulerConfig>()
                .AddTransient<IProcess, SchedulerSchedulerProcessor>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddTransient<IClock, Clock>()
                .AddConditionally(
                "NewScheduler",
                    featureActiveRegistrations =>
                    {
                        featureActiveRegistrations.AddTransient<ISchedulerSchedulerDao, SchedulerSchedulerDaoNew>();
                    },
                    featureInactiveRegistrations =>
                    {
                        featureInactiveRegistrations.AddTransient<ISchedulerSchedulerDao, SchedulerSchedulerDaoOld>();

                    });
        }
    }
}