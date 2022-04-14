using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomClear : ClearSweet
{
    public override void Clear()
    {
        base.Clear();
        GameManager.instance.BoomClear((int)sweet.Pos.x, (int)sweet.Pos.y);
    }
}
