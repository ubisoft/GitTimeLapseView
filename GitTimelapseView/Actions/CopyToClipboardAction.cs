using System;
using System.Threading.Tasks;
using GitTimelapseView.Extensions;
using Humanizer;

namespace GitTimelapseView.Actions
{
    public class CopyToClipboardAction : ActionBase
    {
        private readonly string? _stringToCopy;
        private readonly string _stringToCopyDescription;

        public CopyToClipboardAction(string? stringToCopy, string stringToCopyDescription)
        {
            _stringToCopy = stringToCopy;
            _stringToCopyDescription = stringToCopyDescription;
            DisplayName = "Copy to clipboard";
            SuccessNotificationText = $"{stringToCopyDescription.Transform(To.SentenceCase)} copied to clipboard";
            FailureNotificationText = $"Failed to copy {stringToCopyDescription} to clipboard";
        }

        public override Task ExecuteAsync(IActionContext context)
        {
            if (string.IsNullOrEmpty(_stringToCopy))
            {
                throw new ArgumentNullException($"{_stringToCopyDescription.Transform(To.SentenceCase)} is null or empty");
            }

            System.Windows.Clipboard.SetText(_stringToCopy);
            return Task.CompletedTask;
        }
    }
}
