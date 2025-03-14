// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Windows;
using CommandLine;
using GitTimelapseView.Actions;
using GitTimelapseView.Extensions;
using GitTimelapseView.Helpers;
using GitTimelapseView.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Events;
using Application = System.Windows.Application;

namespace GitTimelapseView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IAppInfo
    {
        private static readonly TimeSpan s_maxLogAgeBeforeCleanup = TimeSpan.FromDays(14);
        private string? _version;

        public App()
        {
            Startup += async (sender, args) => await OnApplicationStartup(sender, args);
        }

        public static new App Current => (App)Application.Current;

        public ServiceProvider ServiceProvider { get; private set; } = new ServiceCollection().BuildServiceProvider();

        public Microsoft.Extensions.Logging.ILogger Logger { get; private set; } = NullLogger.Instance;

        public ILoggerFactory LoggerFactory { get; private set; } = NullLoggerFactory.Instance;

        public string ApplicationName
        {
            get
            {
                var asm = GetType().Assembly;
                var productName = asm.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
                if (!string.IsNullOrEmpty(productName))
                {
                    return productName;
                }

                return asm.GetName().Name ?? nameof(GitTimelapseView);
            }
        }

        public string ApplicationVersion
        {
            get
            {
                if (_version == null)
                {
                    var asm = GetType().Assembly;
                    _version = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
                }

                return _version ?? string.Empty;
            }
        }

        public string[] StartupArguments { get; private set; } = Array.Empty<string>();

        public StartupOptions StartupOptions { get; private set; } = new();

        public string ApplicationDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationName);

        public string LogsPath => Path.Combine(ApplicationDataPath, "Logs");

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureLogger(serviceCollection);
            ConfigureAppInfo(serviceCollection);
            ConfigureBlazor(serviceCollection);
            ConfigureServices(serviceCollection);
            ConfigurePlugins(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private async Task InitializeServicesAsync()
        {
            var services = ServiceProvider.GetServices<IService>();
            foreach (var service in services)
            {
                await service.InitializeAsync();
            }
        }

        private void ConfigureAppInfo(ServiceCollection serviceCollection)
        {
            Logger.LogInformation($"Initializing {ApplicationName} {ApplicationVersion}...");
            if (StartupArguments != null && StartupArguments.Length > 0)
            {
                Logger.LogInformation($"with arguments: \"{string.Join(" ", StartupArguments)}\"");
            }

            serviceCollection.AddSingleton<IAppInfo>(this);
        }

        private void ConfigureBlazor(ServiceCollection serviceCollection)
        {
            serviceCollection
                .AddBlazorWebView()
                .AddAntDesign();
            Logger.LogInformation("Blazor initialized");
        }

        private void ConfigurePlugins(ServiceCollection serviceCollection)
        {
            var pluginService = new PluginService(LoggerFactory.CreateLogger(nameof(PluginService)));
            serviceCollection.AddSingleton(pluginService);
            pluginService.LoadPlugins(serviceCollection);
            Logger.LogInformation("Plugins initialized");
        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            var timelapseService = new TimelapseService(LoggerFactory);
            var userInfoService = new UserInfoService(LoggerFactory, new Lazy<IEnumerable<IUserInfoProvider>>(() => ServiceProvider.GetServices<IUserInfoProvider>()));
            var telemetryService = new TelemetryService(LoggerFactory, new Lazy<IEnumerable<ITelemetryProvider>>(() => ServiceProvider.GetServices<ITelemetryProvider>()), this);
            var messagingService = new MessagingService(LoggerFactory);
            var pageProgressService = new PageProgressService(LoggerFactory);
            var actionService = new ActionService(LoggerFactory, telemetryService, messagingService, pageProgressService);
            var themingService = new ThemingService(LoggerFactory);
            serviceCollection.RegisterService(timelapseService);
            serviceCollection.RegisterService(userInfoService);
            serviceCollection.RegisterService(telemetryService);
            serviceCollection.RegisterService(actionService);
            serviceCollection.RegisterService(themingService);
            serviceCollection.RegisterService(messagingService);
            serviceCollection.RegisterService(pageProgressService);
            Logger.LogInformation("Services initialized");
        }

        private void ConfigureLogger(ServiceCollection serviceCollection)
        {
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}";

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(LogEventLevel.Information)
                .WriteTo.File(Path.Combine(LogsPath, $"{ApplicationName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{System.Environment.ProcessId}.txt"), LogEventLevel.Information, outputTemplate: outputTemplate)
                .CreateLogger();

            LoggerFactory = new LoggerFactory().AddSerilog(loggerConfiguration);
            serviceCollection.AddSingleton(LoggerFactory);

            Logger = LoggerFactory.CreateLogger("Default");
            serviceCollection.AddSingleton(Logger);

            CleanOldLogs();

            Logger.LogDebug("Logger initialized");

            void CleanOldLogs()
            {
                try
                {
                    var directory = new DirectoryInfo(LogsPath);
                    var currentTime = DateTime.UtcNow;
                    var logsToClean = directory.GetFiles("*.txt").Where(x => currentTime - x.LastWriteTime > s_maxLogAgeBeforeCleanup).ToArray();
                    if (logsToClean.Length > 0)
                    {
                        logsToClean.AsParallel().ForAll(x => File.Delete(x.FullName));
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Unable to clean old log files");
                }
            }
        }

        private async Task OnApplicationStartup(object sender, StartupEventArgs startupArguments)
        {
            StartupArguments = startupArguments.Args;
            Parser.Default.ParseArguments<StartupOptions>(StartupArguments)
                .WithParsed(o => { if (o != null) StartupOptions = o; });

            if (StartupArguments != null && StartupArguments.Length == 1 && File.Exists(StartupArguments[0]) && string.IsNullOrEmpty(StartupOptions.InputFile))
            {
                StartupOptions.InputFile = StartupArguments[0];
            }

            ConfigureServices();
            await InitializeServicesAsync();

            var timelapseService = ServiceProvider.GetService<TimelapseService>();
            var themingService = ServiceProvider.GetService<ThemingService>();
            var window = new MainWindow(timelapseService, themingService);
            window.Show();
            Logger.LogInformation("> Application successfully initialized");

            if (timelapseService != null && StartupOptions.InputFile != null)
            {
                await new OpenFileAction(StartupOptions.InputFile, StartupOptions.LineNumber, StartupOptions.RevisionIndex, StartupOptions.Sha).ExecuteAsync().ConfigureAwait(false);
            }
        }
    }
}
