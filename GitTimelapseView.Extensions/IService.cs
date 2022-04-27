// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Extensions
{
    public interface IService
    {
        /// <summary>
        /// Gets a logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets a logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initialization method
        /// </summary>
        /// <returns>an initializaiton task</returns>
        Task InitializeAsync();
    }
}
