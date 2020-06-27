﻿using UnityEngine;

public class WishCard : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI text = null;
    public TMPro.TextMeshProUGUI Text
    {
        get
        {
            return text;
        }
    }

    [SerializeField]
    private Renderer render = null;
    public Material Mat
    {
        get
        {
            return render.material;
        }
    }
}
