using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSweet : MonoBehaviour
{
    public AnimationClip clearAnimation;
    public AudioClip clearAudio;

    public int oneClearedSocre = 1;

    private bool isClearing;
    public bool IsClearing { get => isClearing; }

    protected SweetObject sweet;

    private void Awake()
    {
        sweet = GetComponent<SweetObject>();
    }

    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(clearCoroutine());
    }
    IEnumerator clearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if(animator!= null)
        {
            animator.Play(clearAnimation.name);
            GameManager.instance.Score++;
            AudioSource.PlayClipAtPoint(clearAudio, gameObject.transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
