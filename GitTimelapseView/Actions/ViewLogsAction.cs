// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Extensions;

namespace GitTimelapseView.Actions
{
    internal class ViewLogsAction : ActionBase
    {
        public ViewLogsAction()
        {
            DisplayName = "View Error Logs";
            Tooltip = "View Error Logs";
            Icon = "Text";
        }

        public override Task ExecuteAsync(IActionContext context)
        {
            Process.Start("explorer.exe", App.Current.LogsPath);
            return Task.CompletedTask;
        }
    }
}
