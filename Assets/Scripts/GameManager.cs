using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region//单例模式
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if(instance != this)
            Destroy(gameObject);
    }
    #endregion

    #region // 填充和清除的相关物体
    [System.Serializable]
    public struct SweetInfo
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public List<SweetInfo> sweetsList;
    public Dictionary<SweetsType, GameObject> sweetObjDict;
    public SweetObject[,] sweets;

    private SweetObject pressedSweet;
    private SweetObject enterdSweet;

    public GameObject GridPrefab;
    public GameObject BackGround;

    public int X_num, Y_num;
    public float fillTime = 0.5f;
    public int bisuitNum = 10;

    //控制生成的特殊种类
    private bool IsBoom = false;// 判断爆炸(true)或全消(false)
    private bool IsRow = false;// 判断行(true)或列(false)
    #endregion

    private void Start()
    {
        sweetObjDict = new Dictionary<SweetsType, GameObject>();
        for (int i = 0; i < sweetsList.Capacity; i++)
        {
            if(!sweetObjDict.ContainsKey(sweetsList[i].type))
                sweetObjDict.Add(sweetsList[i].type, sweetsList[i].prefab);
        }

        for (int i = 0; i < X_num; i++)
        {
            for (int j = 0; j < Y_num; j++)
            {
                GameObject tempGrid = Instantiate(GridPrefab, CorrectPos(i, j), Quaternion.identity);
                tempGrid.transform.SetParent(BackGround.transform);
            }
        }

        sweets = new SweetObject[X_num, Y_num];
        for (int i = 0; i < X_num; i++)
        {
            for (int j = 0; j < Y_num; j++)
            {
                CreateNewSweet(i, j, SweetsType.EMPTY);
            }
        }

        SpownBarries();

        StartCoroutine(AllFilled());
    }

    #region //UI界面
    [Header("UI界面相关")]
    public Text timeText;
    [SerializeField]private float overTime = 60f;

    public Text scoreText;
    public float AddingTime = 0.5f;

    private int score;

    public int Score
    {
        get => score;
        set
        {  
            score = value;
            scoreText.text = score.ToString();
        }
    }

    #endregion

    #region//游戏流程控制
    [Header("流程控制相关")]
    public GameObject GameOver_Panel;
    public Text EndingScore_Text;
    [SerializeField]private bool gameOver = false;

    public float OverTime
    {
        get
        {
            if (overTime == 0)
            {
                GameOver();
                gameOver = true;
            }
            return overTime;
        }
        set
        {
            if (overTime <= 0)
                overTime = 0;
            else
                overTime = value;
        }
    }

    private void Update()
    {
        OverTime -= Time.deltaTime;
        timeText.text = OverTime.ToString("0");

    }

    //游戏结束相关操作
    private void GameOver()
    {
        gameOver = true;
        GameOver_Panel.SetActive(true);
        EndingScore_Text.text = Score.ToString();
    }
    #endregion


    #region//场景控制
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void RePlay()
    {
        SceneManager.LoadScene(1);
    }

    #endregion


    #region//生成相关
    public Vector3 CorrectPos(int x, int y)
    {
        return new Vector3(x - X_num / 2 + 0.5f, y - Y_num / 2 + 0.5f, 0);
    }

    public SweetObject CreateNewSweet(int _x, int _y, SweetsType _type)
    {
        GameObject newSweet = Instantiate(sweetObjDict[_type], CorrectPos(_x, _y), Quaternion.identity);
        newSweet.transform.SetParent(BackGround.transform);

        sweets[_x, _y] = newSweet.GetComponent<SweetObject>();
        sweets[_x, _y].Init(_x, _y, _type);

        return sweets[_x, _y];
    }

    private void SpownBarries()
    {
        int i = 0;
        while(i < bisuitNum)
        {
            int x = Random.Range(0, X_num);
            int y = Random.Range(0, Y_num);
            if (sweets[x, y].Type != SweetsType.BARRIER)
            {
                Destroy(sweets[x, y].gameObject);
                CreateNewSweet(x, y, SweetsType.BARRIER);
                i++;
            }
        }
    }
    #endregion
    
    /// <summary>
    /// 填充
    /// </summary>
    /// <returns></returns>
    public IEnumerator AllFilled()
    {
        bool needFill = true;

        while(needFill)
        {
            yield return new WaitForSeconds(fillTime);

            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }

            needFill = ClearMatched();
        }
    }
    public bool Fill()
    {
        bool NotFillComplete = false;
        for (int j = 1; j < Y_num; j++)
        {
            for (int i = 0; i < X_num; i++)
            {
                SweetObject sweet = sweets[i, j];

                if(sweet.CanMove())             //垂直填充
                {
                    //Debug.Log("1");
                    SweetObject sweetBelow = sweets[i, j - 1];

                    if(sweetBelow.Type == SweetsType.EMPTY)
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MoveComponent.OnMove(i, j - 1, fillTime);
                        sweets[i, j - 1] = sweet;

                        CreateNewSweet(i, j, SweetsType.EMPTY);
                        NotFillComplete = true;
                    }
                    else
                    {
                        for (int down = 1; down >= -1; down--)
                        {
                            if (down != 0)
                            {
                                int downX = i + down;
                                if (downX >= 0 && downX < X_num)
                                {
                                    SweetObject downSweet = sweets[downX, j - 1];
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true;
                                        for (int aboveY = j; aboveY < Y_num; aboveY++)
                                        {
                                            SweetObject sweetAbove = sweets[downX, aboveY];
                                            if (sweetAbove.CanMove())
                                                break;
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetsType.EMPTY)
                                            {
                                                canfill = false;
                                                break;
                                            }
                                        }

                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject); 
                                            sweet.MoveComponent.OnMove(downX, j - 1, fillTime);
                                            sweets[downX, j - 1] = sweet;
                                            CreateNewSweet(i, j, SweetsType.EMPTY);
                                            NotFillComplete = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
               
            }
        }

        for (int i = 0; i < X_num; i++)
        {
            SweetObject sweet = sweets[i, Y_num - 1];

            if(sweet.Type == SweetsType.EMPTY)
            {
                
                GameObject newSweet = Instantiate(sweetObjDict[SweetsType.NORMAL], CorrectPos(i, Y_num), Quaternion.identity);
                newSweet.transform.SetParent(BackGround.transform);

                sweets[i, Y_num - 1] = newSweet.GetComponent<SweetObject>();
                sweets[i, Y_num - 1].Init(i, Y_num, SweetsType.NORMAL);
                sweets[i, Y_num - 1].MoveComponent.OnMove(i, Y_num - 1, fillTime);
                sweets[i, Y_num - 1].ColorComponent.SetColor((ColorType)Random.Range(0, sweets[i, Y_num - 1].ColorComponent.NumColors));

                Destroy(sweet.gameObject);

                NotFillComplete = true;
            }
        }
        return NotFillComplete;
    }

    #region//交换位置
    private bool IsNeighbour(SweetObject _sweet1, SweetObject _sweet2)
    {
        return Vector2.SqrMagnitude(_sweet1.Pos - _sweet2.Pos) == 1;
        //return ((_sweet1.Pos - _sweet2.Pos) == Vector2.up) || (_sweet1.Pos - _sweet2.Pos == Vector2.down) || (_sweet1.Pos - _sweet2.Pos == Vector2.right) || (_sweet1.Pos - _sweet2.Pos == Vector2.left);
    }

    public void ExchangeSweet(SweetObject _sweet1, SweetObject _sweet2)
    {
        bool CouldChange = true;
        if(_sweet1.CanMove() && _sweet2.CanMove())
        {
            sweets[(int)_sweet1.Pos.x, (int)_sweet1.Pos.y] = _sweet2;
            sweets[(int)_sweet2.Pos.x, (int)_sweet2.Pos.y] = _sweet1;

            Vector2 tempPos = _sweet2.Pos;

            _sweet2.MoveComponent.OnMove((int)_sweet1.Pos.x, (int)_sweet1.Pos.y, fillTime);
            _sweet1.MoveComponent.OnMove((int)tempPos.x, (int)tempPos.y, fillTime);

            if(_sweet1.Type == SweetsType.RAINBOWCANDY && _sweet2.Type == SweetsType.RAINBOWCANDY)
            {
                for (int i = 0; i < X_num; i++)
                {
                    for (int j = 0; j < Y_num; j++)
                    {
                        if(sweets[i, j].CanClear())
                        {
                            sweets[i, j].ClearComponent.Clear();
                        }
                    }
                }
                CouldChange = false;
            }
           
            if(_sweet1.Type == SweetsType.RAINBOWCANDY && _sweet2.CanClear())
            {
                ClearTypes clearType = _sweet1.GetComponent<ClearTypes>();
                if(clearType != null)
                {
                    clearType.ColorType = _sweet2.ColorComponent.colorType;
                    clearType.SweetsType = _sweet2.Type;
                }
                ClearSweets((int)_sweet1.Pos.x, (int)_sweet1.Pos.y);
                CouldChange = false;
            }

            if (_sweet2.Type == SweetsType.RAINBOWCANDY && _sweet2.CanClear())
            {
                ClearTypes clearType = _sweet2.GetComponent<ClearTypes>();
                if (clearType != null)
                {
                    clearType.ColorType = _sweet1.ColorComponent.colorType;
                    clearType.SweetsType = _sweet2.Type;
                }
                ClearSweets((int)_sweet2.Pos.x, (int)_sweet2.Pos.y);
                CouldChange = false;
            }

            if(_sweet1.Type != SweetsType.NORMAL && _sweet2.Type != SweetsType.NORMAL)
            {
                _sweet1.ClearComponent.Clear();
                _sweet2.ClearComponent.Clear();
                CreateNewSweet((int)_sweet1.Pos.x, (int)_sweet1.Pos.y, SweetsType.EMPTY);
                CouldChange = false;
            }

            if (ClearMatched() || !CouldChange)
                StartCoroutine(AllFilled());
            else 
                StartCoroutine(ChangeBack(_sweet1, _sweet2, CouldChange));
        }
    }
    IEnumerator ChangeBack(SweetObject _sweet1, SweetObject _sweet2, bool _change)
    {
        if (_change)
        {
            yield return new WaitForSeconds(fillTime);
            if (MatchSweets(_sweet1, (int)_sweet1.Pos.x, (int)_sweet1.Pos.y) == null && MatchSweets(_sweet2, (int)_sweet2.Pos.x, (int)_sweet2.Pos.y) == null)
            {
                sweets[(int)_sweet1.Pos.x, (int)_sweet1.Pos.y] = _sweet2;
                sweets[(int)_sweet2.Pos.x, (int)_sweet2.Pos.y] = _sweet1;

                Vector2 tempPos = _sweet2.Pos;

                _sweet2.MoveComponent.OnMove((int)_sweet1.Pos.x, (int)_sweet1.Pos.y, fillTime);
                _sweet1.MoveComponent.OnMove((int)tempPos.x, (int)tempPos.y, fillTime);

            }
        }
    }

    public void PressSweet(SweetObject _sweet)
    {
        if(!gameOver)
            pressedSweet = _sweet;
    }
    public void EnterSweet(SweetObject _sweet)
    {
        if (!gameOver)
            enterdSweet = _sweet;
    }
    public void ReleaseSweet()
    {
        if (!gameOver)
        {
            if (IsNeighbour(pressedSweet, enterdSweet))
                ExchangeSweet(pressedSweet, enterdSweet);
        }
    }
    #endregion

    //匹配算法
    public List<SweetObject> MatchSweets(SweetObject _sweet, int _X, int _Y)
    {
        if(_sweet.CanColor())
        {
            ColorType colorType = _sweet.ColorComponent.colorType;
            List<SweetObject> matchRowSweets = new List<SweetObject>();
            List<SweetObject> matchLineSweets = new List<SweetObject>();
            List<SweetObject> MatchedSweets = new List<SweetObject>();

            #region//行匹配
            matchRowSweets.Add(_sweet);
            for (int i = 0; i <= 1; i++)
            {
                for(int xdis = 1; xdis < X_num; xdis++)
                {
                    int x;
                    if (i == 0)
                        x = _X - xdis;
                    else
                        x = _X + xdis;
                    if (x < 0 || x >= X_num)
                        break;

                    if (sweets[x, _Y].CanColor() && sweets[x, _Y].ColorComponent.ColorType == colorType)
                        matchRowSweets.Add(sweets[x, _Y]);
                    else
                        break;
                }
            }
            if (matchRowSweets.Count >= 3)
            {
                IsRow = true;
                IsBoom = false;
                for (int i = 0; i < matchRowSweets.Count; i++)
                    MatchedSweets.Add(matchRowSweets[i]);

                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int ydis = 1; ydis < Y_num; ydis++)
                        {
                            int y;
                            if (j == 0)
                                y = _Y - ydis;
                            else
                                y = _Y + ydis;

                            if (y < 0 || y >= Y_num)
                                break;

                            if (sweets[(int)matchRowSweets[i].Pos.x, y].CanColor() && sweets[(int)matchRowSweets[i].Pos.x, y].ColorComponent.ColorType == colorType)
                            {
                                matchLineSweets.Add(sweets[(int)matchRowSweets[i].Pos.x, y]);
                            }
                            else
                                break;
                        }
                    }
                    if (matchLineSweets.Count < 2)
                    {
                        matchLineSweets.Clear();    
                    }
                    else
                    {
                        IsBoom = true;
                        for (int j = 0; j < matchLineSweets.Count; j++)
                        {
                            MatchedSweets.Add(matchLineSweets[j]);
                        }
                        break;
                    }
                }
                return MatchedSweets;
            }
            #endregion

            matchRowSweets.Clear();
            matchLineSweets.Clear();
            MatchedSweets.Clear();

            #region//列匹配
            matchLineSweets.Add(_sweet);
            for (int i = 0; i <= 1; i++) 
            {
                for (int ydis = 1; ydis < X_num; ydis++)
                {
                    int y;
                    if (i == 0)
                        y = _Y - ydis;
                    else
                        y = _Y + ydis;
                    if (y < 0 || y >= Y_num)
                        break;

                    if (sweets[_X, y].CanColor() && sweets[_X, y].ColorComponent.ColorType == colorType)
                        matchLineSweets.Add(sweets[_X, y]);
                    else
                        break;
                }
            }
            
            if (matchLineSweets.Count >= 3)
            {
                IsRow = false;
                IsBoom = false;
                for (int i = 0; i < matchLineSweets.Count; i++)
                    MatchedSweets.Add(matchLineSweets[i]);

                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xdis = 1; xdis < X_num; xdis++)
                        {
                            int x;
                            if (j == 0)
                                x = _X - xdis;
                            else
                                x = _X + xdis;

                            if (x < 0 || x >= X_num)
                                break;

                            if (sweets[x, (int)matchLineSweets[i].Pos.y].CanColor() && sweets[x, (int)matchLineSweets[i].Pos.y].ColorComponent.ColorType == colorType)
                            {
                                matchRowSweets.Add(sweets[x, (int)matchLineSweets[i].Pos.y]);
                            }
                            else
                                break;
                        }
                    }
                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        IsBoom = true;
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            MatchedSweets.Add(matchRowSweets[j]);
                        }
                        break;
                    }
                }
                return MatchedSweets;
            }
            #endregion
        }
        return null;
    }

    #region//清除相关
    public bool ClearSweets(int _x, int _y)
    {
        if(sweets[_x, _y].CanClear()&&!sweets[_x,_y].ClearComponent.IsClearing)
        {
            sweets[_x, _y].ClearComponent.Clear();
            CreateNewSweet(_x, _y, SweetsType.EMPTY);
            ClearBarrier(_x, _y);
            return true;
        }
        return false;
    }

    private void ClearBarrier(int _x, int _y)
    {
        for (int x = _x - 1; x <= _x + 1; x++)
        {
            if(x != _x && x < X_num && x>=0)
            {
                if (sweets[x, _y].Type == SweetsType.BARRIER && sweets[x, _y].CanClear())
                {
                    sweets[x, _y].ClearComponent.Clear();
                    CreateNewSweet(x, _y, SweetsType.EMPTY);
                }
            }
        }
        for (int y = _y - 1; y <= _y + 1; y++)
        {
            if (y != _y && y < Y_num && y >= 0)
            {
                if (sweets[_x, y].Type == SweetsType.BARRIER && sweets[_x, y].CanClear())
                {
                    sweets[_x, y].ClearComponent.Clear();
                    CreateNewSweet(_x, y, SweetsType.EMPTY);
                }
            }
        }
    }

    public void RowClear(int row)
    {
        for (int i = 0; i < X_num; i++)
        {
            ClearSweets(i, row);
        }
    }

    public void ColumnClear(int column)
    {
        for (int i = 0; i < Y_num; i++)
        {
            ClearSweets(column, i);
        }
    }

    public void BoomClear(int _x, int _y)
    {
        for (int i = _x - 2; i <= _x + 2; i++)
        {
            for (int j = _y - 2; j <= _y + 2 ; j++)
            {
                if(i >= 0 && i < X_num && j >= 0 && j < Y_num)
                {
                    if((Mathf.Abs(i - _x) + Mathf.Abs(j - _y)) <= 2)
                    {
                        ClearSweets(i, j);
                    }
                }
            }
        }
    }

    public void ClearTypesAll(ColorType _colorType, SweetsType _sweetsType)
    {
        if(_sweetsType != SweetsType.NORMAL)
        {
            for (int i = 0; i < X_num; i++)
            {
                for (int j = 0; j < Y_num; j++)
                {
                    if(sweets[i, j].CanColor() && sweets[i, j].ColorComponent.colorType == _colorType)
                    {
                        Destroy(sweets[i, j].gameObject);
                        sweets[i, j] = null;
                        CreateNewSweet(i, j, _sweetsType);
                        if(sweets[i, j].ColorComponent != null)
                            sweets[i, j].ColorComponent.SetColor(_colorType);
                    }
                }
            }
        }
        for (int i = 0; i < X_num; i++)
        {
            for (int j = 0; j < Y_num; j++)
            {
                if (sweets[i, j].CanColor() && (sweets[i, j].ColorComponent.colorType == _colorType || _colorType == ColorType.COLORS))
                {
                    ClearSweets(i, j);
                }
            }
        }
    }


    private bool ClearMatched()
    {
        bool needFill = false;

        for(int i = 0; i < X_num; i++)
        {
            for (int j = 0; j < Y_num; j++)
            {
                if(sweets[i, j].CanClear())
                {
                    List<SweetObject> matchList = MatchSweets(sweets[i, j], i, j);

                    //对多消进行奖励
                    if(matchList != null)
                        AddScore(matchList.Count);

                    if(matchList != null)
                    {
                        SweetsType specialSweetType = SweetsType.COUNT;
                        if(matchList.Count == 4)
                        {
                            Debug.Log(IsRow);
                            if (IsRow)
                                specialSweetType = SweetsType.ROW_CLEAR;
                            else
                                specialSweetType = SweetsType.COLUMN_CLEAR;
                        }    
                        if(matchList.Count >= 5)
                        {
                            if (IsBoom)
                                specialSweetType = SweetsType.BOOM_CLEAR;
                            else
                                specialSweetType = SweetsType.RAINBOWCANDY;
                        }

                        SweetObject sweetObj = matchList[Random.Range(0, matchList.Count)];
                        int x = (int)sweetObj.Pos.x;
                        int y = (int)sweetObj.Pos.y;

                        for (int z = 0; z < matchList.Count; z++)
                        {
                            if(ClearSweets((int)matchList[z].Pos.x, (int)matchList[z].Pos.y))
                            {
                                needFill = true;
                            }
                        }

                        if(specialSweetType != SweetsType.COUNT)
                        {
                            Destroy(sweets[x, y]);
                            SweetObject newSweet = CreateNewSweet(x, y, specialSweetType);
                            if((specialSweetType == SweetsType.COLUMN_CLEAR || specialSweetType == SweetsType.ROW_CLEAR || specialSweetType == SweetsType.BOOM_CLEAR) && newSweet.CanColor())
                            {
                                newSweet.ColorComponent.SetColor(matchList[0].ColorComponent.colorType);
                            }
                            if(specialSweetType == SweetsType.RAINBOWCANDY && newSweet.CanColor())
                            {
                                newSweet.ColorComponent.SetColor(ColorType.COLORS);
                            }
                        }
                    }
                }
            }
        }
        return needFill;
    }

    //多消加分
    private void AddScore(int _x)
    {
        _x -= 3;
        if (_x == 1)
        {
            Score += 2;
        }
        else if (_x == 2)
        {
            Score += 4;
        }
    }
    #endregion

#if UNITY_EDITOR
    #region //测试
    [Header("测试")]
    public int x;
    public int y;
    public SweetsType sweetType;
    public ColorType sweetcolorType;
    public void GoldFinger()
    {
        Destroy(sweets[x, y].gameObject);
        SweetObject sweet = CreateNewSweet(x, y, sweetType);
        sweet.ColorComponent.SetColor(sweetcolorType);
    }
    #endregion
#endif
}

public enum SweetsType
{
    EMPTY,              //空
    NORMAL,             //普通
    BARRIER,            //障碍
    ROW_CLEAR,          //行消除
    COLUMN_CLEAR,       //列消除
    BOOM_CLEAR,         //爆炸
    RAINBOWCANDY,       //彩虹糖
    COUNT               //标记类型
}