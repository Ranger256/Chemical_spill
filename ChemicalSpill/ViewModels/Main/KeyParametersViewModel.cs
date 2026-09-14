using System.Collections.Generic;
using System.Collections.ObjectModel;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Основные параметры главного экрана: подборка величин, за которыми оператор
    /// следит постоянно. Полный перечень выводится в разделе «Параметры и графики».
    /// </summary>
    public class KeyParametersViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;

        /// <summary>Ключи величин, выводимых плитками на главном экране.</summary>
        private static readonly string[] KeyOrder =
        {
            "o2", "h2o", "pressure", "chamberTemp",
            "productTemp", "density", "doseActual", "doseDeviation",
            "torque", "angle", "throughput", "rejectShare",
            "productLeft", "caps", "gasPressure", "power"
        };

        private readonly Dictionary<string, ParameterItemViewModel> _byKey =
            new Dictionary<string, ParameterItemViewModel>();

        public KeyParametersViewModel(IMachineService machine)
        {
            _machine = machine;
            Items = new ObservableCollection<ParameterItemViewModel>();
            Build();
        }

        public ObservableCollection<ParameterItemViewModel> Items { get; private set; }

        public string GoodCountText { get { return _machine.Batch.Good.ToString(); } }

        public string RejectCountText { get { return _machine.Batch.RejectTotal.ToString(); } }

        public string RotorText
        {
            get { return _machine.Rotor.RotorType + " · позиция " + _machine.Rotor.CurrentPosition; }
        }

        public void Refresh()
        {
            foreach (var snapshot in _machine.GetParameters(null))
            {
                ParameterItemViewModel item;
                if (_byKey.TryGetValue(snapshot.Key, out item)) item.Update(snapshot);
            }

            OnPropertyChanged("GoodCountText");
            OnPropertyChanged("RejectCountText");
            OnPropertyChanged("RotorText");
        }

        private void Build()
        {
            var source = new Dictionary<string, ParameterSnapshot>();
            foreach (var snapshot in _machine.GetParameters(null))
            {
                source[snapshot.Key] = snapshot;
            }

            foreach (var key in KeyOrder)
            {
                ParameterSnapshot snapshot;
                if (!source.TryGetValue(key, out snapshot)) continue;

                var item = new ParameterItemViewModel(snapshot);
                _byKey[key] = item;
                Items.Add(item);
            }
        }
    }
}
