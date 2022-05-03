// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace GitTimelapseView.Extensions
{
    public interface IUserInfoProvider
    {
        /// <summary>
        /// Get info about an user
        /// </summary>
        Task<UserInfo?> GetUserInfoAsync(string email);
    }
}
