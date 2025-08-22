using System;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Mono
{
    public class InputManager: MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (!mainCamera) return;

            var plane = new Plane(mainCamera.transform.forward,Vector3.zero);
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var enter))
            {
                var hit = ray.GetPoint(enter);
                MouseInput.TargetWorld = new float3(hit.x, hit.y, hit.z);
            }

            MouseInput.ShootDown = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        }
    }
    
    public static class MouseInput
    {
        public static float3 TargetWorld;
        public static bool ShootDown;
    }
}