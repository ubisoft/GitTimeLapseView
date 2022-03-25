namespace GitTimelapseView.Extensions
{
    public class UserInfo
    {
        public UserInfo(string email)
        {
            Email = email;
        }

        /// <summary>
        /// Gets the email
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the display name
        /// </summary>
        public string? DisplayName { get; init; }

        /// <summary>
        /// Gets the account name
        /// </summary>
        public string? AccountName { get; init; }

        /// <summary>
        /// Gets the job project
        /// </summary>
        public string? Project { get; init; }

        /// <summary>
        /// Gets the job title
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Gets the location
        /// </summary>
        public string? Location { get; init; }

        /// <summary>
        /// Gets or sets the profile picture url
        /// </summary>
        public string? ProfilePictureUrl { get; set; }
    }
}
