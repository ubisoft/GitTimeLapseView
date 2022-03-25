namespace GitTimelapseView.Data
{
    public enum CommitChangeReason
    {
        /// <summary>
        /// Explicitely triggered by user
        /// </summary>
        Explicit,

        /// <summary>
        /// Triggered during the loading
        /// </summary>
        Loading,
    }
}
