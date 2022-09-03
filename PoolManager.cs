using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PoolManager: MonoBehaviour {
    public Queue<SpriteRenderer> Pool = new Queue<SpriteRenderer>();
    public Transform PoolParent;
    public SpriteRenderer prefab;

    /// <summary>
    /// Tries to get a SpriteRenderer from the pool, if the pool is empty, a new SpriteRenderer is created
    /// </summary>
    /// <param name="sprite">Sprite given to SpriteRenderer</param>
    /// <returns>SpriteRenderer</returns>
    public SpriteRenderer PullSprite(Sprite sprite)
    {
        SpriteRenderer s;
        if (Pool.Count > 0){
            s = Pool.Dequeue();
        }
        else{
            s = Instantiate(prefab.gameObject).GetComponent<SpriteRenderer>();
            s.transform.SetParent(PoolParent);
            s.gameObject.SetActive(true);
        }

        s.sprite = sprite;
        //s.transform.localScale = scale;
        s.enabled = true;
        return s;
    }

    /// <summary>
    /// Marks a SpriteRenderer as no longer in use allowing it to be pulled by other processes
    /// </summary>
    /// <param name="spriteRenderer">The SpriteRenderer to be released</param>
    public void ReleaseSprite(SpriteRenderer spriteRenderer) 
    {
        spriteRenderer.enabled = false;
        Pool.Enqueue(spriteRenderer);
    }

    private void LateUpdate()
    {
        
    }
}