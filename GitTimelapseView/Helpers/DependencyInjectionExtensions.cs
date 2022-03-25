using GitTimelapseView.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GitTimelapseView.Helpers
{
    internal static class DependencyInjectionExtensions
    {
        public static void RegisterService<TService>(this ServiceCollection serviceCollection, TService service)
            where TService : class, IService
        {
            serviceCollection.AddSingleton(service);
            serviceCollection.AddSingleton<IService>(service);
        }
    }
}
