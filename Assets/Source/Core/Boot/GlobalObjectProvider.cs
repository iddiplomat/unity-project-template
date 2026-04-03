using UnityEngine;

namespace Source.Boot
{
    public sealed class GlobalObjectProvider
    {
        public Transform CanvasOverlay { get; private set; }
        public Transform CanvasCamera { get; private set; }

        public void Initialize(Transform canvasOverlay, Transform canvasCamera)
        {
            CanvasOverlay = canvasOverlay;
            CanvasCamera = canvasCamera;

            Object.DontDestroyOnLoad(CanvasOverlay);
            Object.DontDestroyOnLoad(CanvasCamera);
        }
    }
}