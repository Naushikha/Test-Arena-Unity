using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitForSplashEnd : MonoBehaviour
{
    public float videoTime = 6f;
    void Start()
    {
        StartCoroutine(WaitForVideoEnd());
    }

    IEnumerator WaitForVideoEnd()
    {
        yield return new WaitForSeconds(videoTime);
        SceneManager.LoadScene("menu");
    }
}
