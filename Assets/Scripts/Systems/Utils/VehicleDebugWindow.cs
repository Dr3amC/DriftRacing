#if UNITY_EDITOR
using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Systems.Utils
{
    public class VehicleDebugWindow : EditorWindow
    {
        [SerializeField] private WheelData[] wheels;

        private Editor _editor;

        private void OnEnable()
        {
            _editor = Editor.CreateEditor(this);
        }

        private void OnGUI()
        {
            _editor.OnInspectorGUI();
        }

        [MenuItem("Debug/Vehicle")]
        private static void ShowWindow()
        {
            var window = GetWindow<VehicleDebugWindow>();
            window.Show();
        }

        public void SetWheel(int index, WheelData wheel)
        {
            if (wheels == null)
            {
                wheels = new WheelData[index + 1];
            }
            else if (wheels.Length <= index)
            {
                Array.Resize(ref wheels, index + 1);
            }

            wheels[index] = wheel;
        }

        [Serializable]
        public struct WheelData
        {
            public float Slip;
            public float Torque;
            public float3 Impulse;
            public float SuspensionImpulse;
            public float RotationSpeed;
        }
    }
}
#endif