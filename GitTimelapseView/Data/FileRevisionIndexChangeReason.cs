namespace GitTimelapseView.Data
{
    public enum FileRevisionIndexChangeReason
    {
        /// <summary>
        /// Explicitely triggered by the user
        /// </summary>
        Explicit,

        /// <summary>
        /// Triggered during loading
        /// </summary>
        Loading,
    }
}
