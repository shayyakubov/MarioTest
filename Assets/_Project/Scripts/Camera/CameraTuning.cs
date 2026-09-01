using System;
using UnityEngine;

namespace MarioTest.Camera
{
    [Serializable]
    public sealed class CameraTuning
    {
        [SerializeField] private float _distance = 10f;
        [SerializeField] private float _height = 6f;
        [SerializeField] private float _pitch = 25f;
        [SerializeField] private float _pivotHeight = 1f;
        [SerializeField] private float _smoothTimeHorizontal = 0.2f;
        [SerializeField] private float _smoothTimeVertical = 0.5f;
        [SerializeField] private float _leadDistance = 1.5f;
        [SerializeField] private float _leadMinSpeed = 0.5f;
        [SerializeField] private float _maxCameraSpeed;

        public float Distance => _distance;
        public float Height => _height;
        public float Pitch => _pitch;
        public float PivotHeight => _pivotHeight;
        public float SmoothTimeHorizontal => _smoothTimeHorizontal;
        public float SmoothTimeVertical => _smoothTimeVertical;
        public float LeadDistance => _leadDistance;
        public float LeadMinSpeed => _leadMinSpeed;
        public float MaxCameraSpeed => _maxCameraSpeed;
    }
}
