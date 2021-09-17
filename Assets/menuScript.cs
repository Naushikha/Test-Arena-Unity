using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menuScript : MonoBehaviour
{
    public Image player;

    public Image title;
    public Sprite defTitle;
    public Sprite[] titleList;
    public AudioSource[] titleSFXList;
    public Sprite[] animList;
    public int[] breakList;
    public float frameTime = 0.5f;
    public float breakTime = 3f;
    private int currentAnim = 0;
    private int currentBreak = 0;
    private List<int> breakFrame = new List<int>();
    void Start()
    {
        // Populate the breakpoints according to cumulative frames
        int currFrame = 0;
        foreach (int breakF in breakList)
        {
            currFrame += breakF;
            breakFrame.Add(currFrame);
        }
        StartCoroutine(runAnim());
        StartCoroutine(glitchTitle());
    }
    IEnumerator runAnim()
    {
        player.sprite = animList[currentAnim];
        currentAnim++;
        // If it's a new animation, take a break
        if (breakFrame[currentBreak] == currentAnim)
        {
            Color tmpCol = player.color;
            tmpCol.a = 0;
            player.color = tmpCol;
            currentBreak++;
            yield return new WaitForSeconds(breakTime);
            tmpCol.a = 100;
            player.sprite = null;
            player.color = tmpCol;
            // Last frame is gonna be a break frame anyway
            if (currentAnim == animList.Length)
            {
                currentBreak = 0;
                currentAnim = 0;
            }
        }
        yield return new WaitForSeconds(frameTime);
        StartCoroutine(runAnim());
    }

    IEnumerator glitchTitle()
    {
        yield return new WaitForSeconds(Random.Range(3, 7));
        int rand = Random.Range(0, titleList.Length);
        title.sprite = titleList[rand];
        titleSFXList[rand].Play();
        yield return new WaitForSeconds(0.2f);
        title.sprite = defTitle;
        StartCoroutine(glitchTitle());
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void playGameScene()
    {
        SceneManager.LoadScene("game");
    }

    public void exitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
