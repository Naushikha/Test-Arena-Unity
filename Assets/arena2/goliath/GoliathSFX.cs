using UnityEngine;

public class GoliathSFX : MonoBehaviour
{
    public AudioSource sfxStartStep, sfxPlaceStep, sfxFire, sfxShot, sfxDie;
    public AudioClip[] clipShot;
    public AudioClip clipFireCharge, clipFire;
    public void placeStep() { sfxPlaceStep.Play(); }
    public void startStep() { sfxStartStep.Play(); }
    public void fireCharge() { sfxFire.PlayOneShot(clipFireCharge, 1.0f); }
    public void fireChargeStop() { sfxFire.Stop(); }
    public void fire() { sfxFire.PlayOneShot(clipFire, 0.7f); }
    public void shot() { sfxShot.PlayOneShot(clipShot[Random.Range(0, clipShot.Length)], 1.0f); }
    public void die() { sfxDie.Play(); }

}
