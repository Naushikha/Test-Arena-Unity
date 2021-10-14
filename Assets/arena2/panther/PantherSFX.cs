using UnityEngine;

public class PantherSFX : MonoBehaviour
{
    public AudioSource sfxIdle1, sfxIdle2, sfxSeen, sfxRun, sfxWalk;
    public void idle()
    {
        if (Random.value >= 0.9) sfxIdle1.Play();
        else if (Random.value >= 0.9) sfxIdle2.Play();
    }
    public void seen() { sfxSeen.Play(); }
    public void run() { sfxRun.Play(); }
    public void walk() { sfxWalk.Play(); }
    // public void startStep() { sfxStartStep.Play(); }
    // public void fireCharge() { sfxFire.PlayOneShot(clipFireCharge, 1.0f); }
    // public void fireChargeStop() { sfxFire.Stop(); }
    // public void fire() { sfxFire.PlayOneShot(clipFire, 0.7f); }
}

