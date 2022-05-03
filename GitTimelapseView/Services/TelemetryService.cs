// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Extensions;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class TelemetryService : ServiceBase
    {
        private readonly Lazy<IEnumerable<ITelemetryProvider>> _providers;
        private readonly IAppInfo _appInfo;

        public TelemetryService(ILoggerFactory loggerFactory, Lazy<IEnumerable<ITelemetryProvider>> providers, IAppInfo appInfo)
            : base(loggerFactory)
        {
            _providers = providers;
            _appInfo = appInfo;
        }

        public override async Task InitializeAsync()
        {
            foreach (var provider in _providers.Value)
            {
                await provider.InitializeAsync(Logger, _appInfo).ConfigureAwait(false);
            }
        }

        public void SetAdditionalInfo(string propertyName, object value)
        {
            foreach (var provider in _providers.Value)
            {
                provider.SetAdditionalInfo(propertyName, value);
            }
        }

        public void TrackPageView(string pageName, TimeSpan loadingTime = default, Uri? pageUrl = null)
        {
            foreach (var provider in _providers.Value)
            {
                provider.TrackPageView(pageName, loadingTime, pageUrl);
            }
        }

        public void TrackEvent(string name, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            foreach (var provider in _providers.Value)
            {
                provider.TrackEvent(name, properties, metrics);
            }
        }

        public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
        {
            foreach (var provider in _providers.Value)
            {
                provider.TrackMetric(name, value, properties);
            }
        }

        public void TrackException(Exception exception)
        {
            foreach (var provider in _providers.Value)
            {
                provider.TrackException(exception);
            }
        }

        public void NotifyUserActivity()
        {
            foreach (var provider in _providers.Value)
            {
                provider.NotifyUserActivity();
            }
        }
    }
}
