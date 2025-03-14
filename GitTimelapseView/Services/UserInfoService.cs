// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using System.Text;
using GitTimelapseView.Extensions;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class UserInfoService : ServiceBase
    {
        public UserInfoService(ILoggerFactory loggerFactory, Lazy<IEnumerable<IUserInfoProvider>> providers)
            : base(loggerFactory)
        {
            Providers = providers;
        }

        public Lazy<IEnumerable<IUserInfoProvider>> Providers { get; }

        public async Task<UserInfo> GetUserInfoAsync(string email)
        {
            UserInfo? userInfo = null;
            foreach (var provider in Providers.Value)
            {
                try
                {
                    userInfo = await provider.GetUserInfoAsync(email).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Unable to get user info from email '{email}' with provider '{provider.GetType().Name}'");
                }

                if (userInfo != null)
                    break;
            }

            if (userInfo == null)
            {
                userInfo = new UserInfo(email);
            }

            if (string.IsNullOrEmpty(userInfo.ProfilePictureUrl))
            {
                userInfo.ProfilePictureUrl = GetGravatarProfileUrl(email);
            }

            return userInfo;
        }

        private static string GetGravatarProfileUrl(string email)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(email.ToLowerInvariant().Trim(' '));
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", hashBytes[i]);
            }

            var hash = sb.ToString().ToLowerInvariant();
            return $"https://www.gravatar.com/avatar/{hash}";
        }
    }
}
