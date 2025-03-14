// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows;
using AntDesign;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class MessagingService : ServiceBase
    {
        public MessagingService(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        internal MessageService? MessageService { get; set; }

        public void ShowInformationDialog(string errorMessage, string? title = null)
        {
            MessageBox.Show(errorMessage, title ?? App.Current.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal void Success(string message)
        {
            _ = MessageService?.Success(message);
        }

        internal void Error(string message)
        {
            _ = MessageService?.Error(message);
        }
    }
}
