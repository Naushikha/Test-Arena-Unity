using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menuScript : MonoBehaviour
{
    public Image player;
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
    // Update is called once per frame
    void Update()
    {

    }

    public void playGameScene()
    {
        Debug.Log("clicked this nigga");
        SceneManager.LoadScene("game");
    }
}
