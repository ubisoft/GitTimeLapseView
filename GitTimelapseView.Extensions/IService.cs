using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Extensions
{
    public interface IService
    {
        /// <summary>
        /// Gets a logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets a logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initialization method
        /// </summary>
        /// <returns>an initializaiton task</returns>
        Task InitializeAsync();
    }
}
