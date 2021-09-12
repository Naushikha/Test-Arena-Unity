using UnityEngine;

public class gunAnimSFXSync : MonoBehaviour
{
    // Put here temporarily for reloading sound effects
    void clipin()
    {
        transform.Find("SFX/clipin").GetComponent<AudioSource>().Play();
    }
    void clipout()
    {
        transform.Find("SFX/clipout").GetComponent<AudioSource>().Play();

    }
    // MP5
    void slideback()
    {
        transform.Find("SFX/slideback").GetComponent<AudioSource>().Play();

    }
    // M4A1
    void boltpull()
    {
        transform.Find("SFX/boltpull").GetComponent<AudioSource>().Play();

    }
    void draw()
    {
        transform.Find("SFX/draw").GetComponent<AudioSource>().Play();
    }
    void fire()
    {
        AudioSource SFX_fire = transform.Find("SFX/fire").GetComponent<AudioSource>();
        SFX_fire.pitch = Random.Range(0.95f, 1.05f); // Give the sound a little bit of variation
        SFX_fire.Play();
    }
}
