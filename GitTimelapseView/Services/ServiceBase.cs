﻿// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Extensions;
using GitTimelapseView.Helpers;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public abstract class ServiceBase : BindableBase, IService
    {
        protected ServiceBase(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            Logger = loggerFactory.CreateLogger(GetType().Name);
        }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger Logger { get; }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
