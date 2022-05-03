// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitTimelapseView.Helpers
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void ObservePropertyChanged(string propertyName, EventHandler<PropertyChangedEventArgs> handler, bool addOrRemove = true)
        {
            if (addOrRemove)
            {
                PropertyChangedEventManager.AddHandler(this, handler, propertyName);
            }
            else
            {
                PropertyChangedEventManager.RemoveHandler(this, handler, propertyName);
            }
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
