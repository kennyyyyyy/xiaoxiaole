using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSweet : MonoBehaviour
{
    private SweetObject sweet;

    private IEnumerator moveCoroutine;

    private void Awake()
    {
        sweet = GetComponent<SweetObject>();
    }

    public void OnMove(int _newX, int _newY, float _time)
    {
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(_newX, _newY, _time);
        StartCoroutine(moveCoroutine);
    }

    private IEnumerator MoveCoroutine(int _newX, int _newY, float _time)
    {
        sweet.Pos = new Vector3(_newX, _newY);
        
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPos(_newX, _newY);

        for (float i = 0; i < _time; i += Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos, endPos, i / _time);
            yield return null;
        }

        sweet.transform.position = endPos;
    }
}
