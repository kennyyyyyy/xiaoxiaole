using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLineSweets : ClearSweet
{
    [SerializeField]private ClearType type;

    public override void Clear()
    {
        base.Clear();
        switch(type)
        {
            case ClearType.ROWCLEAR:
                {
                    GameManager.instance.RowClear((int)sweet.Pos.y);
                    break;
                }
            case ClearType.COLUMNCLEAR:
                {
                    GameManager.instance.ColumnClear((int)sweet.Pos.x);
                    break;
                }
        }
    }
}

enum ClearType
{
    ROWCLEAR,
    COLUMNCLEAR,
}
