using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSweet : MonoBehaviour
{
    private List<ColorSprite> colorSprites;

    private Dictionary<ColorType, Sprite> colorSpriteDict;

    private SpriteRenderer sprite;

    public int NumColors
    {
        get => colorSprites.Capacity;
    }

    public ColorType ColorType { get => colorType; set => SetColor(value); }
    public ColorType colorType;

    private void Awake()
    {
        colorSprites = GameManager.instance.transform.GetComponent<SweetColorType>().ColorSpriteList;

        sprite = transform.Find("Sweet").GetComponent<SpriteRenderer>();

        colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for (int i = 0; i < colorSprites.Capacity; i++)
        {
            if(!colorSpriteDict.ContainsKey(colorSprites[i].type))
            {
                colorSpriteDict.Add(colorSprites[i].type, colorSprites[i].sprite);
            }
        }

    }
    
    public void SetColor(ColorType _colorType)
    {
        if(colorSpriteDict.ContainsKey(_colorType))
        {
            colorType = _colorType;
            sprite.sprite = colorSpriteDict[_colorType];
        }
    }
}
