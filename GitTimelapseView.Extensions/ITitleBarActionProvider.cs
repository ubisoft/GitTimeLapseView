// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace GitTimelapseView.Extensions
{
    public interface ITitleBarActionProvider
    {
        /// <summary>
        /// Provide actions that will be added to title bar
        /// </summary>
        IEnumerable<IAction> GetActions();
    }
}
