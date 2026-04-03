using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Source.Infrastructure.MVVM.UI;
using UnityEngine;

namespace Source.Infrastructure.MVVM
{
    public class ScreenRegistry : IScreenRegistry
    {
        private readonly Dictionary<UIScreenType, ScreenBinding> _bindings = new();

        public ScreenRegistry()
        {
            ScanAssemblies();
        }

        public ScreenBinding GetBinding(UIScreenType screenType)
        {
            if (_bindings.TryGetValue(screenType, out var binding))
                return binding;

            throw new InvalidOperationException(
                $"No screen binding found for {screenType}. " +
                $"Add [ScreenBinding({screenType}, typeof(YourViewModel))] to your View class.");
        }

        private void ScanAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<ScreenBindingAttribute>();

                    if (attribute == null)
                        continue;

                    if (_bindings.ContainsKey(attribute.ScreenType))
                    {
                        Debug.LogError(
                            $"Duplicate ScreenBinding for {attribute.ScreenType}: " +
                            $"{type.Name} and {_bindings[attribute.ScreenType].ViewType.Name}");
                        continue;
                    }

                    _bindings[attribute.ScreenType] = new ScreenBinding(type, attribute.ViewModelType);
                }
            }
        }
    }

    public readonly struct ScreenBinding
    {
        public Type ViewType { get; }
        public Type ViewModelType { get; }

        public ScreenBinding(Type viewType, Type viewModelType)
        {
            ViewType = viewType;
            ViewModelType = viewModelType;
        }
    }

    public interface IScreenRegistry
    {
        ScreenBinding GetBinding(UIScreenType screenType);
    }
}
