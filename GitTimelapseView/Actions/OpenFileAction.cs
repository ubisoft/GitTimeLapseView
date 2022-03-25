using System.Threading.Tasks;
using GitTimelapseView.Extensions;
using GitTimelapseView.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace GitTimelapseView.Actions
{
    public class OpenFileAction : ActionBase
    {
        private readonly string? _filePath;
        private readonly int? _lineNumber;
        private readonly int? _revisionIndex;
        private readonly string? _revisionSha;

        public OpenFileAction(string? filePath = null, int? lineNumber = null, int? revisionIndex = null, string? revisionSha = null)
        {
            DisplayName = "Open File...";
            Tooltip = "View history of any file from a git repository";
            Icon = "FolderOpen";
            InputGestureText = "Ctrl+O";

            _filePath = filePath;
            _lineNumber = lineNumber;
            _revisionIndex = revisionIndex;
            _revisionSha = revisionSha;
        }

        public override async Task ExecuteAsync(IActionContext context)
        {
            context.ProgressFeedback = VisualFeedback.None;
            var timelapseService = App.Current.ServiceProvider.GetService<TimelapseService>();
            if (timelapseService == null)
            {
                context.LogError($"Unable to get {nameof(TimelapseService)}");
                return;
            }

            var filePath = _filePath;
            if (string.IsNullOrEmpty(filePath))
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog(App.Current.MainWindow) == true)
                    {
                        filePath = openFileDialog.FileName;
                    }
                });
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                context.LogInformation($"Opening file '{filePath}'");
                context.ProgressMessage = $"Please wait while loading '{filePath}'...";
                context.ProgressFeedback = VisualFeedback.FullScreen;
                context.TrackingProperties["FilePath"] = filePath;
                context.ErrorFeedback = VisualFeedback.FullScreen;
                await timelapseService.OpenFileAsync(context, filePath, _lineNumber, _revisionIndex, _revisionSha).ConfigureAwait(false);
            }
        }
    }
}
