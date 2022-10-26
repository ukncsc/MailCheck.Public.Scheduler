using System;
using MailCheck.Common.Messaging;
using Microsoft.Extensions.CommandLineUtils;

namespace MailCheck.Scheduler.Scheduler
{
    public static class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication(false)
            {
                Name = "SchedulerScheduler"
            };

            app.Command("lambda-cw", LambdaCloudWatch);
            app.Execute(args);
        }

        private static readonly Action<CommandLineApplication> LambdaCloudWatch = command =>
        {
            command.Description = "Execute CloudWatch triggered SPF scheduler lambda code locally.";

            command.OnExecute(async () =>
            {
                LambdaEntryPoint entryPoint = new LambdaEntryPoint();

                await entryPoint.FunctionHandler(new ScheduledEvent("","",null,"",DateTime.Now, new Guid(),null ), LambdaContext.NonExpiringLambda);

                return 0;
            });
        };
    }
}
