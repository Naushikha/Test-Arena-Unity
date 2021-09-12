using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class hudBlood : MonoBehaviour
{
    public float timePerIter = 0.05f;
    public RawImage bloodSplatter;
    public RectTransform bloodSplatterTransform;
    public AudioSource[] SFX_hurt;

    void OnEnable()
    {
        // Rotate blood texture randomly
        int x = 0, y = 0, z = 0;
        if (Random.value >= 0.5) x = 180;
        if (Random.value >= 0.5) y = 180;
        if (Random.value >= 0.5) z = 180;
        bloodSplatterTransform.eulerAngles = new Vector3(x, y, z);
        SFX_hurt[Random.Range(0, SFX_hurt.Length)].Play();
        StartCoroutine(fadeBlood());
    }
    IEnumerator fadeBlood()
    {
        for (float f = 1f; f >= -0.05f; f -= 0.05f)
        {
            Color tmp = bloodSplatter.color;
            tmp.a = f;
            bloodSplatter.color = tmp;
            yield return new WaitForSeconds(timePerIter);
        }
        this.gameObject.SetActive(false);
    }
}
