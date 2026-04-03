using System;

namespace Source.Extensions
{
    public class ReactiveProperty<TProperty>
    {
        private TProperty _value;

        public event Action<TProperty> OnChanged;

        public TProperty Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;

                _value = value;
                OnChanged?.Invoke(_value);
            }
        }

        public ReactiveProperty() { }

        public ReactiveProperty(TProperty initialValue)
        {
            _value = initialValue;
        }

        public static implicit operator TProperty(ReactiveProperty<TProperty> property)
        {
            return property.Value;
        }
    }
}