using Hank.ColorPicker;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Image img;
    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            EyeDropper.Pick(
                (c) =>
                {
                    img.sprite = null;
                    img.color = c;
                },
                () =>
                {
                    img.sprite = null;
                    img.color = Color.white;
                },
                (s, c) =>
                {
                    img.color = Color.white;
                    img.sprite = s;
                }
            );
        });
    }

}
