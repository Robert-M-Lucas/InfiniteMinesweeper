using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PoolManager: MonoBehaviour {
    public Queue<SpriteRenderer> Pool = new Queue<SpriteRenderer>();
    public Transform PoolParent;
    public SpriteRenderer prefab;

    public SpriteRenderer PullSprite(Sprite sprite)
    {
        SpriteRenderer s;
        if (Pool.Count > 0){
            s = Pool.Dequeue();
        }
        else{
            s = Instantiate(prefab.gameObject).GetComponent<SpriteRenderer>();
            s.transform.SetParent(PoolParent);
        }

        s.sprite = sprite;
        //s.transform.localScale = scale;
        s.gameObject.SetActive(true);
        return s;
    }

    public void ReleaseSprite(SpriteRenderer s) 
    {
        s.gameObject.SetActive(false);
        Pool.Enqueue(s);
    }
}