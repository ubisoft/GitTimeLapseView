// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class PageProgressService : ServiceBase
    {
        private bool _isProgressVisible;

        public PageProgressService(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value);
        }
    }
}
