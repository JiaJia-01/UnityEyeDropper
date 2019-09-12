#if UNITY_EDITOR
using UnityEditor;

namespace Hank.ColorPicker
{
    [InitializeOnLoad]
    public class EditorPlayModeStateChanged
    {
        static EditorPlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += PlayModeState;
        }

        private static void PlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
                EyeDropperWindow.OnExitingPlayMode();
        }
    }

}

#endif
