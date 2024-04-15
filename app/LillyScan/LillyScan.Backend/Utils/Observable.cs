using LillyScan.Backend.Parallelization;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Utils
{
    public class Observable<T>
    {
        private readonly Atomic<T> pValue = new Atomic<T>();
        public T Value
        {
            get => pValue.Get();
            set
            {
                pValue.With(_ =>
                {
                    pValue.Value = value;
                    ValueChanged?.Invoke(this, value);
                });
            }
        }

        public delegate void OnValueChanged(Observable<T> observable, T newValue);
        public event OnValueChanged ValueChanged;

    }
}
