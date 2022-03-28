using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App1.ViewModels
{

    public class Bindable : INotifyPropertyChanged
    {
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            field = value;
            NotifyPropertyChanged(propertyName);
        }
    }
}
