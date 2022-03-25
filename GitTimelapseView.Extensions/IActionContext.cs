using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Extensions
{
    public interface IActionContext : ILogger
    {
        /// <summary>
        /// Current state
        /// </summary>
        ActionState State { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to track this action or not
        /// </summary>
        bool IsTrackingEnabled { get; set; }

        /// <summary>
        /// Gets or sets an Id used for tracking. Defaults to action type name.
        /// </summary>
        string TrackingId { get; set; }

        /// <summary>
        /// Gets a dictionary with tracking properties that can be set
        /// </summary>
        IDictionary<string, string> TrackingProperties { get; }

        /// <summary>
        /// Gets a dictionary Tracking metric that can be set
        /// </summary>
        IDictionary<string, double> TrackingMetrics { get; }

        /// <summary>
        /// Gets or sets a value indicating the type of progress feedback
        /// </summary>
        VisualFeedback ProgressFeedback { get; set; }

        /// <summary>
        /// Gets or sets a message shown during progress
        /// </summary>
        string? ProgressMessage { get; set; }

        /// <summary>
        /// Gets or sets the last error message
        /// </summary>
        string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the last exception
        /// </summary>
        Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of error feedback
        /// </summary>
        VisualFeedback ErrorFeedback { get; set; }
    }
}
