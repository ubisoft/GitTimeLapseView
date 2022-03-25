using System.Threading.Tasks;

namespace GitTimelapseView.Extensions
{
    public interface IUserInfoProvider
    {
        /// <summary>
        /// Get info about an user
        /// </summary>
        Task<UserInfo?> GetUserInfoAsync(string email);
    }
}
