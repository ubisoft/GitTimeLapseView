using System.Globalization;
using System.Threading.Tasks;
using GitTimelapseView.Extensions;
using GitTimelapseView.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Actions
{
    public class ChangeCurrentRevisionAction : ActionBase
    {
        private readonly int _revisionIndex;

        public ChangeCurrentRevisionAction(int revisionIndex)
        {
            _revisionIndex = revisionIndex;
            DisplayName = "Change current Revision";
            Tooltip = "View history of any file from a git repository";
            Icon = "DateRange";
        }

        public override async Task ExecuteAsync(IActionContext context)
        {
            context.LogInformation($"Selecting revision '{_revisionIndex}'");
            context.TrackingProperties["RevisionIndex"] = _revisionIndex.ToString(CultureInfo.InvariantCulture);
            var timelapseService = App.Current.ServiceProvider.GetService<TimelapseService>();
            if (timelapseService == null)
            {
                context.LogError($"Unable to get {nameof(TimelapseService)}");
                return;
            }

            await timelapseService.SetCurrentFileRevisionIndexAsync(context, _revisionIndex).ConfigureAwait(false);
        }
    }
}
