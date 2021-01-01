using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public Image fadeplane;
    public GameObject gameOverUI;
    
    private void Start()
    {
        FindObjectOfType<Player>().OnDeath += onGameOver;
    }

    void onGameOver()
    {
        StartCoroutine(fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadeplane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }
    
    //Input

    public void StartNewGame()
    {
        Application.LoadLevel("Game");
    }
}
