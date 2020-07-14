using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SharedViewModels.Properties;

namespace SharedViewModels.ViewModels
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        protected readonly Dictionary<string, object> backingFields = new Dictionary<string, object>();
        protected bool suppressPropertyChangedEvents = false;
        public event PropertyChangedEventHandler PropertyChanged;

        protected T GetValue<T>([CallerMemberName]string propertyName = null)
        {
            if (!backingFields.ContainsKey(propertyName))
                return default;
            return (T)backingFields[propertyName];
        }
        protected void SetValue<T>(T item, [CallerMemberName] string propertyName = null)
        {
            backingFields[propertyName] = item;
            OnPropertyChanged(propertyName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(suppressPropertyChangedEvents)
                return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
