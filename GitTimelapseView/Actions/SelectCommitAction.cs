using System.Threading.Tasks;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Extensions;
using GitTimelapseView.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Actions
{
    internal class SelectCommitAction : ActionBase
    {
        private readonly Commit _commit;

        public SelectCommitAction(Commit commit)
        {
            _commit = commit;
            DisplayName = "Select Commit";
            Tooltip = "Select a commit in the UI";
            Icon = "SourceCommit";
        }

        public override async Task ExecuteAsync(IActionContext context)
        {
            context.LogInformation($"Selecting commit '{_commit.Id}'");
            context.IsTrackingEnabled = false;
            var timelapseService = App.Current.ServiceProvider.GetService<TimelapseService>();
            if (timelapseService == null)
            {
                context.LogError($"Unable to get {nameof(TimelapseService)}");
                return;
            }

            await timelapseService.SetCurrentCommitAsync(context, _commit).ConfigureAwait(false);
        }
    }
}
