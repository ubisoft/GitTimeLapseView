using System.Threading.Tasks;
using GitTimelapseView.Extensions;
using GitTimelapseView.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitTimelapseView.Helpers
{
    public static class ActionExtensions
    {
        public static Task ExecuteAsync(this IAction action)
        {
            return App.Current.ServiceProvider.GetService<ActionService>()?.ExecuteAsync(action) ?? Task.CompletedTask;
        }
    }
}
