﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject flashHolder;
    public float flashTime;
    public Sprite[] flashSprites;
    public SpriteRenderer[] spriteRenderers;

    private void Start()
    {
        deactive();
    }

    public void activate()
    {
        flashHolder.SetActive(true);

        int flashSpriteIndex = UnityEngine.Random.Range(0, flashSprites.Length);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
        }
        
        Invoke("deactive", flashTime);
    }

    void deactive()
    {
        flashHolder.SetActive(false);
    }
}
