using System;
using UnityEngine;

namespace Hank.ColorPicker
{
    public class EyeDropper
    {
        public static void Pick(Action<Color> onConfirm, Action onCancel, Action<Sprite, Color> onChange)
        {
            GameObject obj = new GameObject("EyeDropper", typeof(EyeDropperBehaviour));
            EyeDropperBehaviour behaviour = obj.GetComponent<EyeDropperBehaviour>();
            behaviour.onCancel = () =>
            {
                behaviour.enabled = false;
                UnityEngine.Object.Destroy(obj);
                if (onCancel != null) onCancel.Invoke();
            };
            behaviour.onChange = onChange;
            behaviour.onConfirm = (c) =>
            {
                behaviour.enabled = false;
                UnityEngine.Object.Destroy(obj);
                if (onConfirm != null) onConfirm.Invoke(c);
            };
        }
    }

    internal class EyeDropperBehaviour : MonoBehaviour
    {

        public Action<Color> onConfirm;
        public Action<Sprite, Color> onChange;
        public Action onCancel;

        void Awake()
        {
            EyeDropperWindow.Open();
        }

        void Update()
        {
            var state = EyeDropperWindow.Status;

            if (state == EyeDropperWindow.EPickStatus.Picking)
            {
                if (EyeDropperWindow.Dirty && onChange != null)
                {
                    onChange.Invoke(EyeDropperWindow.CurrentSprite, EyeDropperWindow.CurrentColor);
                }
            }
            else if (state == EyeDropperWindow.EPickStatus.Canceled)
            {
                EyeDropperWindow.Status = EyeDropperWindow.EPickStatus.None;
                onCancel.Invoke();
            }
            else if (state == EyeDropperWindow.EPickStatus.Picked)
            {
                EyeDropperWindow.Status = EyeDropperWindow.EPickStatus.None;
                onConfirm.Invoke(EyeDropperWindow.CurrentColor);
            }

        }

    }
}

