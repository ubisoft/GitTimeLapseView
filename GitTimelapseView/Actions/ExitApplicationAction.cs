// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Extensions;

namespace GitTimelapseView.Actions
{
    internal class ExitApplicationAction : ActionBase
    {
        public ExitApplicationAction()
        {
            DisplayName = "Exit";
            Tooltip = "Exit the application";
            Icon = null;
            InputGestureText = "Alt+F4";
        }

        public override Task ExecuteAsync(IActionContext context)
        {
            context.IsTrackingEnabled = false;
            ((MainWindow)App.Current.MainWindow).ExitApplication();
            return Task.CompletedTask;
        }
    }
}
