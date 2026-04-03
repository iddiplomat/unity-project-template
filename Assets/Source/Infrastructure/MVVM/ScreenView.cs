using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Source.Infrastructure.MVVM
{
    public abstract class ScreenView<TViewModel> : MonoBehaviour, IDisposable, IScreenView
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float showScreenDuration = 0.3f;
        
        protected TViewModel ViewModel { get; private set; }
        
        private Sequence _showSequence;
        private Sequence _hideSequence;

        public event Action OnShowStarted;
        public event Action OnShowCompleted;
        public event Action OnHideStarted;
        public event Action OnHideCompleted;

        protected virtual void Awake()
        {
            canvasGroup.alpha = 0;
        }

        public void SetupShowAnimation()
        {
            _showSequence = DOTween.Sequence();
            _showSequence.AppendCallback(() => OnShowStarted?.Invoke());
            _showSequence.Append(DOTween.To(() => canvasGroup.alpha, (x) => canvasGroup.alpha = x, 1.0f, showScreenDuration));
            _showSequence.AppendCallback(() => OnShowCompleted?.Invoke());
        }
        
        public void SetupHideAnimation()
        {
            _showSequence = DOTween.Sequence();
            _showSequence.AppendCallback(() => OnHideStarted?.Invoke());
            _showSequence.Append(DOTween.To(() => canvasGroup.alpha, (x) => canvasGroup.alpha = x, 0.0f, showScreenDuration));
            _showSequence.AppendCallback(() => OnHideCompleted?.Invoke());
        }

        void IScreenView.Initialize(IScreenViewModel viewModel)
        {
            Initialize((TViewModel)viewModel);
        }

        public void Initialize(TViewModel viewModel)
        {
            ViewModel = viewModel;
            OnBind();
        }

        protected abstract void OnBind();

        public virtual async UniTask Show()
        {
            await _showSequence.Play();
        }

        public virtual async UniTask Hide()
        {
            await _hideSequence.Play();
        }

        public virtual void Dispose()
        {
        }
    }
}
