using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public float difficulty = 1.0f;
    public bool legacyArena = false;

    public void setDifficulty(float difficulty)
    {
        this.difficulty = difficulty;
    }

    public int currentLevel = 0;
    public int score = 0;
    public int aliensKilled = 0;
}
