using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Extensions
{
    public interface ITelemetryProvider
    {
        /// <summary>
        /// Initialization method
        /// </summary>
        public Task InitializeAsync(ILogger logger, IAppInfo appInfo);

        /// <summary>
        /// Add or update a global property that will be automatically added to all the events you send
        /// </summary>
        /// <remarks>Do not add too many as it will increase the bandwidth used by the telemetry service</remarks>
        void SetAdditionalInfo(string propertyName, object value);

        /// <summary>
        /// Track application page views
        /// </summary>
        void TrackPageView(string pageName, TimeSpan loadingTime = default, Uri? pageUrl = null);

        /// <summary>
        /// Track custom events (like action's calls)
        /// </summary>
        void TrackEvent(string name, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

        /// <summary>
        /// Track any kind of metrics (like cpu or memory usages)
        /// </summary>
        void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);

        /// <summary>
        /// Track any kind of exceptions
        /// </summary>
        void TrackException(Exception exception);

        /// <summary>
        /// Notify user activity (like an keyboard press or mouse click)
        /// </summary>
        void NotifyUserActivity();
    }
}
