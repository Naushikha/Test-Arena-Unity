using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class scoreScript : MonoBehaviour
{
    public int goodScoreMin;
    public string[] phraseGoodArray;
    public string[] phraseBadArray;
    public Text phrase;
    public Text level;
    public Text killed;
    public Text score;

    void Start()
    {
        phrase.text = selectPhrase();
        level.text = "LEVELS SURVIVED | " + GameManager.Instance.currentLevel;
        killed.text = "WARPERS KILLED | " + GameManager.Instance.aliensKilled;
        score.text = "TOTAL SCORE | " + GameManager.Instance.score;
        // Reset all values
        GameManager.Instance.aliensKilled = 0;
        GameManager.Instance.score = 0;
    }

    string selectPhrase()
    {
        if (GameManager.Instance.score > goodScoreMin)
        {
            return phraseGoodArray[Random.Range(0, phraseGoodArray.Length)];
        }
        else
        {
            return phraseBadArray[Random.Range(0, phraseBadArray.Length)];
        }
    }

    public void backToMenu()
    {
        SceneManager.LoadScene("menu");
    }

    public void playAgain()
    {
        // Pick what arena
        if (GameManager.Instance.legacyArena) SceneManager.LoadScene("game");
        else SceneManager.LoadScene("arena2");
    }
}
