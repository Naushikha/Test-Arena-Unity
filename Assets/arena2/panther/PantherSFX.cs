using UnityEngine;

public class PantherSFX : MonoBehaviour
{
    public AudioSource sfxIdle1, sfxIdle2, sfxSeen, sfxRun, sfxWalk, sfxWhip, sfxDie;
    public AudioSource[] sfxHit;
    public void idle()
    {
        if (Random.value >= 0.9) sfxIdle1.Play();
        else if (Random.value >= 0.9) sfxIdle2.Play();
    }
    public void seen() { sfxSeen.Play(); }
    public void run() { sfxRun.Play(); }
    public void walk() { sfxWalk.Play(); }
    public void attack() { sfxWhip.Play(); }
    public void die() { sfxDie.Play(); }
    public void shot() { sfxHit[Random.Range(1, sfxHit.Length)].Play(); }

}

