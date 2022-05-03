// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Text;
using GitTimelapseView.Extensions;
using GitTimelapseView.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitTimelapseView.Actions
{
    internal class AboutAction : ActionBase
    {
        public AboutAction()
        {
            DisplayName = "About";
            Tooltip = $"About {nameof(GitTimelapseView)}";
            Icon = "InfoCircleOutline";
        }

        public override Task ExecuteAsync(IActionContext context)
        {
            var application = App.Current;
            if (application == null)
                return Task.CompletedTask;

            var sb = new StringBuilder();
            sb.AppendLine($"Version: {application.ApplicationVersion}");
            var messagingService = application.ServiceProvider.GetService<MessagingService>();
            if (messagingService != null)
            {
                messagingService.ShowInformationDialog(sb.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
