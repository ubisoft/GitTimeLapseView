// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.DependencyInjection;

namespace GitTimelapseView.Extensions
{
    public interface IPlugin
    {
        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="serviceCollection">a collection of services.</param>
        void ConfigureServices(ServiceCollection serviceCollection);
    }
}
