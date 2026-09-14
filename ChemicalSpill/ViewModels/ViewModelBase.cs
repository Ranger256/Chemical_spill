using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChemicalSpill.ViewModels
{
    /// <summary>
    /// Базовый класс модели представления: уведомление об изменении свойств.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Присваивает значение и уведомляет представление, если оно изменилось.</summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
