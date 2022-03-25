namespace GitTimelapseView.Extensions
{
    public interface IAppInfo
    {
        /// <summary>
        /// Gets the name of the application
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the version of the application
        /// </summary>
        string ApplicationVersion { get; }

        /// <summary>
        /// Gets the application data path
        /// </summary>
        string ApplicationDataPath { get; }

        /// <summary>
        /// Gets the log path
        /// </summary>
        string LogsPath { get; }
    }
}
