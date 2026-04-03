using System;
using Source.Infrastructure.MVVM.UI;

namespace Source.Infrastructure.MVVM
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ScreenBindingAttribute : Attribute
    {
        public UIScreenType ScreenType { get; }
        public Type ViewModelType { get; }

        public ScreenBindingAttribute(UIScreenType screenType, Type viewModelType)
        {
            ScreenType = screenType;
            ViewModelType = viewModelType;
        }
    }
}
