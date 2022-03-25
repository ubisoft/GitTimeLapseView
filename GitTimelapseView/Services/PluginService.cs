using System;
using System.IO;
using System.Linq;
using System.Reflection;
using GitTimelapseView.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    internal class PluginService
    {
        private readonly ILogger _logger;

        public PluginService(ILogger logger)
        {
            _logger = logger;
        }

        internal void LoadPlugins(ServiceCollection serviceCollection)
        {
            var pluginsDirectory = Path.GetDirectoryName(typeof(PluginService).Assembly.Location);
            if (pluginsDirectory == null)
            {
                _logger.LogError($"Unable to load plugins");
                return;
            }

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName?.Contains(nameof(GitTimelapseView), StringComparison.Ordinal) ?? false).ToList();

            var matchingAssemblyFiles = Directory
                .EnumerateFiles(path: pluginsDirectory, searchPattern: $"{nameof(GitTimelapseView)}.*.dll", searchOption: SearchOption.AllDirectories)
                .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).Select(f => f.First());

            foreach (var assemblyFilePath in matchingAssemblyFiles)
            {
                var assemblyFileName = Path.GetFileName(assemblyFilePath);
                if (loadedAssemblies.All(a => !string.Equals(a.ManifestModule.Name, assemblyFileName, StringComparison.Ordinal)))
                    _ = Assembly.LoadFrom(assemblyFilePath);
            }

            try
            {
                var pluginAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && IsPlugin(a)).ToList();
                foreach (var assembly in pluginAssemblies)
                {
                    LoadPlugin(assembly, serviceCollection);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to load plugins");
            }
        }

        private static bool IsPlugin(Assembly assembly)
        {
            return assembly.ExportedTypes.Any(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsAbstract);
        }

        private void LoadPlugin(Assembly assembly, ServiceCollection serviceCollection)
        {
            var pluginClasses = assembly.GetExportedTypes().Where(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsAbstract).ToArray();
            if (pluginClasses.Length == 1)
            {
                try
                {
                    var plugin = Activator.CreateInstance(pluginClasses[0]) as IPlugin;
                    if (plugin == null)
                    {
                        _logger.LogError($"Unable to instantiate plugin '{assembly.GetName().Name}'");
                        return;
                    }

                    plugin.ConfigureServices(serviceCollection);
                    _logger.LogInformation($"Plugin '{assembly.GetName().Name}' Loaded");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unable to load plugin '{assembly.GetName().Name}'");
                }
            }
        }
    }
}
