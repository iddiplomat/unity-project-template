using NUnit.Framework;
using Source.Infrastructure.MVVM;
using UnityEngine;

namespace Source.Infrastructure.MVVM.Tests
{
    public sealed class ScreenViewTests
    {
        [Test]
        public void Initialize_AssignsViewModel_InvokesOnBind()
        {
            var go = new GameObject("ScreenViewTests");
            go.AddComponent<CanvasGroup>();
            BindTestView view = go.AddComponent<BindTestView>();
            var viewModel = new BindTestViewModel();

            try
            {
                view.Initialize(viewModel);

                Assert.That(view.BoundViewModel, Is.SameAs(viewModel));
                Assert.That(view.OnBindCallCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Initialize_FromIScreenView_CastsToConcreteViewModel()
        {
            var go = new GameObject("ScreenViewTests_IScreenView");
            go.AddComponent<CanvasGroup>();
            BindTestView view = go.AddComponent<BindTestView>();
            var viewModel = new BindTestViewModel();

            try
            {
                ((IScreenView)view).Initialize(viewModel);

                Assert.That(view.BoundViewModel, Is.SameAs(viewModel));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private sealed class BindTestViewModel : IScreenViewModel
        {
            public void Initialize()
            {
            }

            public void Dispose()
            {
            }
        }

        private sealed class BindTestView : ScreenView<BindTestViewModel>
        {
            public BindTestViewModel BoundViewModel { get; private set; }
            public int OnBindCallCount { get; private set; }

            protected override void Awake()
            {
                canvasGroup = GetComponent<CanvasGroup>();
                base.Awake();
            }

            protected override void OnBind()
            {
                BoundViewModel = ViewModel;
                OnBindCallCount++;
            }
        }
    }
}
