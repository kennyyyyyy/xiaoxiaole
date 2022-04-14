using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearTypes : ClearSweet
{
    private ColorType colorType = ColorType.COLORS;
    public ColorType ColorType { get => colorType; set => colorType = value; }

    private SweetsType sweetsType = SweetsType.EMPTY;
    public SweetsType SweetsType { get => sweetsType; set => sweetsType = value; }
    
    public override void Clear()
    {
        base.Clear();
        GameManager.instance.ClearTypesAll(colorType, SweetsType);
    }
}
