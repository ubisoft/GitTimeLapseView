using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitTimelapseView.Extensions;
using GitTimelapseView.Helpers;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class ActionService : ServiceBase
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly TelemetryService _telemetryService;
        private readonly PageProgressService _pageProgressService;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private ActionContext? _currentAction;

        public ActionService(ILoggerFactory loggerFactory, TelemetryService telemetryService, MessagingService messagingService, PageProgressService pageProgressService)
            : base(loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger(nameof(ActionService));
            _telemetryService = telemetryService;
            MessagingService = messagingService;
            _pageProgressService = pageProgressService;
        }

        public event EventHandler<ActionContext>? ActionStarted;

        public event EventHandler<ActionContext>? ActionCompleted;

        public ActionContext? CurrentAction
        {
            get => _currentAction;
            set => SetProperty(ref _currentAction, value);
        }

        public MessagingService MessagingService { get; }

        public async Task ExecuteAsync(IAction action)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            var stopwatch = Stopwatch.StartNew();
            var context = new ActionContext(_loggerFactory, action);
            context.ObservePropertyChanged(nameof(IActionContext.ProgressFeedback), (e, args) => _pageProgressService.IsProgressVisible = context.ProgressFeedback == VisualFeedback.ProgressBarTop);
            context.State = ActionState.Running;
            CurrentAction = context;
            ActionStarted?.Invoke(this, context);
            if (context.ProgressFeedback == VisualFeedback.ProgressBarTop)
            {
                _pageProgressService.IsProgressVisible = true;
            }

            try
            {
                await action.ExecuteAsync(context).ConfigureAwait(false);
                if (action.SuccessNotificationText != null)
                {
                    MessagingService.Success(action.SuccessNotificationText);
                }

                context.State = ActionState.Success;
            }
            catch (Exception e)
            {
                context.ErrorMessage ??= $"Unable to perform action {action.GetType().Name}";
                context.Exception = e;
                context.State = ActionState.Failed;
                Logger.LogError(e, $"Unable to perform action {action.GetType().Name}");
                if (context.ErrorFeedback == VisualFeedback.Message)
                {
                    MessagingService.Error(action.FailureNotificationText != null ? $"{action.FailureNotificationText}. {e}" : $"Unable to perform action {action.GetType().Name}. {e}");
                }

                _telemetryService.TrackException(e);
            }

            if (context.IsTrackingEnabled)
            {
                context.TrackingProperties["ActionStatusText"] = context.State == ActionState.Success ? "Ok" : "Error";
                context.TrackingMetrics["ActionExecutionTime"] = stopwatch.ElapsedMilliseconds;
                _telemetryService.TrackEvent(context.TrackingId, context.TrackingProperties, context.TrackingMetrics);
            }

            context.ProgressMessage = null;
            _pageProgressService.IsProgressVisible = false;

            CurrentAction = null;
            ActionCompleted?.Invoke(this, context);
            _semaphoreSlim.Release();
        }

        public class ActionContext : BindableBase, IActionContext
        {
            private readonly ILogger _logger;
            private VisualFeedback _progressFeedback = VisualFeedback.ProgressBarTop;

            public ActionContext(ILoggerFactory loggerFactory, IAction action)
            {
                _logger = loggerFactory.CreateLogger(action.GetType().Name);
                TrackingId = action.GetType().Name.Replace("Action", string.Empty, StringComparison.Ordinal);
                Action = action;
                ProgressMessage = $"'{action.GetType().Name}' in progress";
            }

            public string TrackingId { get; set; }

            public IDictionary<string, string> TrackingProperties { get; } = new Dictionary<string, string>();

            public IDictionary<string, double> TrackingMetrics { get; } = new Dictionary<string, double>();

            public bool IsTrackingEnabled { get; set; } = true;

            public IAction Action { get; }

            public VisualFeedback ProgressFeedback
            {
                get => _progressFeedback;
                set => SetProperty(ref _progressFeedback, value);
            }

            public string? ErrorMessage { get; set; }

            public string? ProgressMessage { get; set; }

            public Exception? Exception { get; set; }

            public VisualFeedback ErrorFeedback { get; set; } = VisualFeedback.Message;

            public ActionState State { get; set; } = ActionState.Unknown;

            public IDisposable BeginScope<TState>(TState state)
            {
                return _logger.BeginScope(state);
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return _logger.IsEnabled(logLevel);
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
                if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
                {
                    var message = formatter(state, exception);
                    ErrorMessage = message;
                    Exception = exception;
                }
            }
        }
    }
}
