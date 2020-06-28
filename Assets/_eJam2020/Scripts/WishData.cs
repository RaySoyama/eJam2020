using Newtonsoft.Json;
using UnityEngine;

public class WishData
{
    public struct ColorVal
    {
        public ColorVal(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        public ColorVal(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public float r;
        public float g;
        public float b;
        public float a;
    }

    public string userID = "";
    [JsonIgnore]
    public Color color
    {
        get
        {
            return new Color(colorVal.r, colorVal.g, colorVal.b, colorVal.a);
        }
    }

    public ColorVal colorVal = new ColorVal(1.0f, 1.0f, 1.0f, 1.0f);

    public string userText = "";
}
