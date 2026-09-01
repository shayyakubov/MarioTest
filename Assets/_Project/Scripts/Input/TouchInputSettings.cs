using System;
using UnityEngine;

namespace MarioTest.Input
{
    [Serializable]
    public sealed class TouchInputSettings
    {
        [SerializeField] [Range(0.25f, 0.75f)] private float _leftScreenFraction = 0.5f;
        [SerializeField] [Range(0.05f, 0.2f)] private float _joystickRadiusScreenFraction = 0.1f;
        [SerializeField] private float _joystickDeadzone = 0.1f;

        public float LeftScreenFraction => _leftScreenFraction;
        public float JoystickDeadzone => _joystickDeadzone;

        public float GetJoystickRadiusPixels()
        {
            Rect safeArea = Screen.safeArea;
            float minDimension = Mathf.Min(safeArea.width, safeArea.height);
            return minDimension * _joystickRadiusScreenFraction;
        }

        public float GetScreenSplitX()
        {
            Rect safeArea = Screen.safeArea;
            return safeArea.xMin + safeArea.width * _leftScreenFraction;
        }

        public bool IsLeftScreen(Vector2 screenPosition)
        {
            return screenPosition.x < GetScreenSplitX();
        }

        public bool IsRightScreen(Vector2 screenPosition)
        {
            return screenPosition.x >= GetScreenSplitX();
        }
    }
}
