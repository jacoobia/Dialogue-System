using UnityEditor;
using UnityEngine;

namespace Megingjord.Shared.Editor.Utils {
    public class SceneUtils {

        public static Vector3 GetPositionForwardFromCamera() {
            var camera = SceneView.lastActiveSceneView.camera.transform;
            var direction = camera.forward.normalized;
            var cameraPosition = camera.position;
            return cameraPosition + direction * 10.0f;
        }
        
    }
}