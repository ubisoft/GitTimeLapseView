// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace GitTimelapseView.Data
{
    public class FileRevisionIndexChangedEventArgs : EventArgs
    {
        public FileRevisionIndexChangedEventArgs(int index, FileRevisionIndexChangeReason reason)
        {
            Index = index;
            Reason = reason;
        }

        public int Index { get; set; }

        public FileRevisionIndexChangeReason Reason { get; set; }
    }
}
