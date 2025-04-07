using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DavidUtilities.CustomGUI
{
    public enum ButtonSize { Small, Medium, Large, Gigantic, Custom }

    public static class DavidGUI
    {
        static Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

        public static bool Button(
            string text,
            ButtonSize size = ButtonSize.Medium,
            float customSize = 0f,
            Color? normalColor = null,
            Color? hoverColor = null,
            Color? pressedColor = null,
            Color? textColor = null,
            Texture2D icon = null,
            TextAnchor iconAlignment = TextAnchor.MiddleCenter)
        {
            var (width, height) = GetButtonDimensions(size, customSize);
            Rect rect = EditorGUILayout.GetControlRect(false, height, GUILayout.Width(width));
            return DrawButton(rect, text, normalColor, hoverColor, pressedColor, textColor, icon, iconAlignment);
        }

        static bool DrawButton(
            Rect rect,
            string text,
            Color? normalColor,
            Color? hoverColor,
            Color? pressedColor,
            Color? textColor,
            Texture2D icon,
            TextAnchor iconAlignment)
        {
            Color cNormal   = normalColor  ?? new Color(0.55f, 0.55f, 0.55f);
            Color cHover    = hoverColor   ?? new Color(0.45f, 0.45f, 0.45f);
            Color cPressed  = pressedColor ?? new Color(0.35f, 0.35f, 0.35f);
            Color cText     = textColor    ?? Color.white;

            Event e = Event.current;
            bool isHovering = rect.Contains(e.mousePosition);
            Color backgroundColor = cNormal;

            if (isHovering)
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    backgroundColor = cPressed;
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    backgroundColor = cPressed;
                }
                else
                {
                    backgroundColor = cHover;
                }
            }

            DrawRoundedRect(rect, backgroundColor, 8);

            GUIContent content = icon != null ? new GUIContent(text, icon) : new GUIContent(text);
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = iconAlignment,
                fontSize = 12
            };
            style.normal.textColor = cText;
            GUI.Label(rect, content, style);

            if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl != 0 && isHovering)
            {
                GUIUtility.hotControl = 0;
                e.Use();
                return true;
            }
            if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl != 0)
            {
                GUIUtility.hotControl = 0;
            }
            return false;
        }

        static (float width, float height) GetButtonDimensions(ButtonSize size, float customSize = 0f)
        {
            switch (size)
            {
                case ButtonSize.Small:
                    return (EditorGUIUtility.currentViewWidth * 0.3f, 20f);
                case ButtonSize.Medium:
                    return (EditorGUIUtility.currentViewWidth * 0.5f, 30f);
                case ButtonSize.Large:
                    return (EditorGUIUtility.currentViewWidth * 0.7f, 40f);
                case ButtonSize.Gigantic:
                    return (EditorGUIUtility.currentViewWidth * 0.9f, 50f);
                case ButtonSize.Custom:
                    return (EditorGUIUtility.currentViewWidth * 0.5f, customSize);
                default:
                    return (EditorGUIUtility.currentViewWidth * 0.5f, 30f);
            }
        }

        static void DrawRoundedRect(Rect rect, Color fillColor, int cornerRadius)
        {
            int w = Mathf.RoundToInt(rect.width);
            int h = Mathf.RoundToInt(rect.height);

            // Upscale for smoother edges
            int upscale = 2;
            int texW = w * upscale;
            int texH = h * upscale;
            int r = cornerRadius * upscale;

            string key = texW + "x" + texH + "_" + r + "_" + fillColor;
            if (!_textureCache.TryGetValue(key, out Texture2D tex))
            {
                tex = GenerateRoundedRectTexture(texW, texH, r, fillColor);
                _textureCache[key] = tex;
            }
            GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill);
        }

        static Texture2D GenerateRoundedRectTexture(int width, int height, int radius, Color fillColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool cornerTL = (x < radius && y < radius);
                    bool cornerTR = (x >= width - radius && y < radius);
                    bool cornerBL = (x < radius && y >= height - radius);
                    bool cornerBR = (x >= width - radius && y >= height - radius);

                    if (cornerTL && Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) > radius ||
                        cornerTR && Vector2.Distance(new Vector2(x, y), new Vector2(width - radius - 1, radius)) > radius ||
                        cornerBL && Vector2.Distance(new Vector2(x, y), new Vector2(radius, height - radius - 1)) > radius ||
                        cornerBR && Vector2.Distance(new Vector2(x, y), new Vector2(width - radius - 1, height - radius - 1)) > radius)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
            }
            tex.Apply(true);
            return tex;
        }
    }
}
