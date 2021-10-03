using UnityEngine;

public class GoliathSFX : MonoBehaviour
{
    public AudioSource sfxStartStep, sfxPlaceStep, sfxFire;
    public AudioClip clipFireCharge, clipFire;
    public void placeStep() { sfxPlaceStep.Play(); }
    public void startStep() { sfxStartStep.Play(); }
    public void fireCharge() { sfxFire.PlayOneShot(clipFireCharge, 1.0f); }
    public void fireChargeStop() { sfxFire.Stop(); }
    public void fire() { sfxFire.PlayOneShot(clipFire, 0.7f); }
}
