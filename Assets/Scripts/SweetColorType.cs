using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorType
{
    BLUE,
    GREEN,
    PINK,
    PURPLE,
    RED,
    YELLOW,
    COLORS,
    COUNT
}

[System.Serializable]
public struct ColorSprite
{
    public ColorType type;
    public Sprite sprite;
}

public class SweetColorType : MonoBehaviour
{
    public List<ColorSprite> ColorSpriteList;
}
