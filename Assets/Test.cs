using Hank.ColorPicker;
using System.IO;
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


        //var b1 = File.ReadAllBytes(@"c:\Users\Hank\Desktop\q\1.cur");
        //var b2 = File.ReadAllBytes(@"c:\Users\Hank\Desktop\q\EyeDropper.cur");

        //var s1 = ZZ(b1);
        //var s2 = ZZ(b2);
        //b1 = null;
    }


    string ZZ(byte[] data)
    {
        string str = "";
        for (int i = 0; i < data.Length; i++)
        {
            str += data[i] + ",";
        }
        return str;
    }

}
