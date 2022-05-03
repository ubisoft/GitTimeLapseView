// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace GitTimelapseView.Data
{
    public enum FileRevisionIndexChangeReason
    {
        /// <summary>
        /// Explicitely triggered by the user
        /// </summary>
        Explicit,

        /// <summary>
        /// Triggered during loading
        /// </summary>
        Loading,
    }
}
