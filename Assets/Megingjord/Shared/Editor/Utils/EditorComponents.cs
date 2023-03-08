using UnityEditor;
using UnityEngine;

namespace Megingjord.Shared.Editor.Utils {
    public abstract class EditorComponents {
        private static readonly Color DefaultColour = new (.3f, .3f, .3f, 1.0f);

        /// <summary>
        /// Renders an editor GUI divider line
        /// </summary>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="padding">UI Padding</param>
        public static void DrawUIDivider(int thickness = 2, int padding = 10) {
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2.0f;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, DefaultColour);
        }
    }
}