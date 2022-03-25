using System;
using GitTimelapseView.Core.Models;

namespace GitTimelapseView.Data
{
    public class CommitChangedEventArgs : EventArgs
    {
        public CommitChangedEventArgs(Commit commit, CommitChangeReason reason)
        {
            Commit = commit;
            Reason = reason;
        }

        public Commit Commit { get; set; }

        public CommitChangeReason Reason { get; set; }
    }
}
