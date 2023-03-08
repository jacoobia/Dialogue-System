using UnityEditor;
using UnityEngine;

namespace Megingjord.Shared.Editor.Utils {
    public static class SceneUtils {

        private const float DistanceFromCamera = 10.0f;
        
        /// <summary>
        /// Gets the position forward from the editor camera by 10 units
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetPositionForwardFromCamera() {
            var camera = SceneView.lastActiveSceneView.camera.transform;
            var direction = camera.forward.normalized;
            var cameraPosition = camera.position;
            return cameraPosition + direction * DistanceFromCamera;
        }
        
    }
}