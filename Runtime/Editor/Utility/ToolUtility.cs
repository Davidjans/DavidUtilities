using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

public static class ToolUtility
{
    #if UNITY_EDITOR
    static GUIStyle GradientButtonStyle = CreateGradientButtonStyle(
        new Color(0.2f, 0.55f, 0.9f),  
        new Color(0.1f, 0.35f, 0.7f),  
        Color.white                    
    );

    public static GUIStyle GetEnabledButtonStyle(Color topColor = default, Color bottomColor = default, Color textColor = default)
    {
        if(topColor == default)
            topColor = new Color(0.5f, 0.7f, 0.5f);
        if(bottomColor == default)
            bottomColor = new Color(0.65f, 0.7f, 0.5f);
        if(textColor == default)
            textColor = Color.white;
        return CreateGradientButtonStyle(topColor, bottomColor, textColor);
    }
    public static GUIStyle GetDisabledButtonStyle(Color topColor = default, Color bottomColor = default, Color textColor = default)
    {
        if(topColor == default)
            topColor = new Color(0.25f, 0.25f, 0.25f);
        if(bottomColor == default)
            bottomColor = new Color(0.38f, 0.38f, 0.38f);
        if(textColor == default)
            textColor = Color.grey;
        return CreateGradientButtonStyle(topColor, bottomColor, textColor);
    }
    
    public static GUIStyle CreateGradientButtonStyle(Color topColor, Color bottomColor, Color textColor)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                background = MakeGradientTex(2, 16, topColor, bottomColor),
                textColor  = textColor
            },
            hover =
            {
                background = MakeGradientTex(2, 16, topColor * 0.9f, bottomColor * 0.9f),
                textColor  = textColor
            },
            active =
            {
                background = MakeGradientTex(2, 16, topColor * 1.1f, bottomColor * 1.1f),
                textColor  = textColor
            },
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding   = new RectOffset(10, 10, 5, 5),
            margin    = new RectOffset(5, 5, 2, 2)
        };

        return style;
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    
    private static Texture2D MakeGradientTex(int width, int height, Color topColor, Color bottomColor)
    {
        Texture2D tex = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color c = Color.Lerp(topColor, bottomColor, t);
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    
    public static void CreateVerticalBox(GUIStyle style)
    {
        GUIStyle boxStyle = CreateBoxStyle();
        GUILayout.BeginVertical(boxStyle);
    }
    
    public static void CreateHorizontalBox(GUIStyle style)
    {
        GUIStyle boxStyle = CreateBoxStyle();
        GUILayout.BeginHorizontal(boxStyle);
    }
    public static void CreateHorizontalBox()
    {
        CreateHorizontalBox(CreateBoxStyle());
    }
    public static void CreateVerticalBox()
    {
        CreateVerticalBox(CreateBoxStyle());
    }
    
    public static void EndVerticalBox()
    {
        GUILayout.EndVertical();
    }
    public static void EndHorizontalBox()
    {
        GUILayout.EndHorizontal();
    }
    public static GUIStyle CreateBoxStyle()
    {
        return CreateBoxStyle(new RectOffset(10, 10, 15, 15),new RectOffset(5, 5, 15, 15));
    }
    public static GUIStyle CreateBoxStyle(RectOffset padding,RectOffset margin)
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = padding; 
        boxStyle.margin = margin;
        return boxStyle;
    }
#endif
}
