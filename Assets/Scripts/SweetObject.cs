using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetObject : MonoBehaviour
{
    public Vector2 Pos 
    { 
        get => pos;
        set 
        {
            if(CanMove())
            {
                pos = value;
            }
        }  
    }
    private Vector3 pos;

    public SweetsType Type { get => type; set => type = value; }
    private SweetsType type;

    [HideInInspector] public GameManager gameManager;

    public MoveSweet MoveComponent { get => moveSweet;}
    private MoveSweet moveSweet;

    public ColorSweet ColorComponent { get => colorSweet; }
    private ColorSweet colorSweet;

    public ClearSweet ClearComponent { get => clearSweet; }
    private ClearSweet clearSweet;

    private void Awake()
    {
        gameManager = GameManager.instance;
        moveSweet = GetComponent<MoveSweet>();
        colorSweet = GetComponent<ColorSweet>();
        clearSweet = GetComponent<ClearSweet>();
    }

    public bool CanMove()
    {
        return moveSweet != null;
    }
    public bool CanColor()
    {
        return ColorComponent != null;
    }
    public bool CanClear()
    {
        return ClearComponent != null;
    }

    public void Init(int _x, int _y, SweetsType _type)
    {
        pos = new Vector3(_x, _y);
        type = _type;
    }

    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }

    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }

    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
