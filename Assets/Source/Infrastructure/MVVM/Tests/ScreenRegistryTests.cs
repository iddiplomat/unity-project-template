using System;
using NUnit.Framework;
using Source.Infrastructure.MVVM;
using Source.Infrastructure.MVVM.UI;

namespace Source.Infrastructure.MVVM.Tests
{
    public sealed class ScreenRegistryTests
    {
        [Test]
        public void GetBinding_ForAttributedScreen_ReturnsTypesFromAttribute()
        {
            var registry = new ScreenRegistry();
            ScreenBinding binding = registry.GetBinding(UIScreenType.CreateSessionPopUp);

            Assert.That(binding.ViewType, Is.EqualTo(typeof(RegistryMarkedView)));
            Assert.That(binding.ViewModelType, Is.EqualTo(typeof(RegistryMarkedViewModel)));
        }

        [Test]
        public void GetBinding_NoBindingForEnumValue_ThrowsInvalidOperationException()
        {
            var registry = new ScreenRegistry();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                registry.GetBinding(UIScreenType.None));

            Assert.That(ex.Message, Does.Contain(nameof(UIScreenType.None)));
        }

        [ScreenBinding(UIScreenType.CreateSessionPopUp, typeof(RegistryMarkedViewModel))]
        private sealed class RegistryMarkedView : ScreenView<RegistryMarkedViewModel>
        {
            protected override void OnBind()
            {
            }
        }

        private sealed class RegistryMarkedViewModel : IScreenViewModel
        {
            public void Initialize()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
