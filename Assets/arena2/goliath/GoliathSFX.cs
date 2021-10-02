using UnityEngine;

public class GoliathSFX : MonoBehaviour
{
    public AudioSource sfxPlaceStep, sfxStartStep;
    void placeStep()
    {
        sfxPlaceStep.Play();
    }
    void startStep()
    {
        sfxStartStep.Play();
    }
}
