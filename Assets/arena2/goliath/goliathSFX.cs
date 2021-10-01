using UnityEngine;

public class goliathSFX : MonoBehaviour
{
    void placeStep()
    {
        transform.Find("SFX/placeStep").GetComponent<AudioSource>().Play();
    }
    void startStep()
    {
        transform.Find("SFX/startStep").GetComponent<AudioSource>().Play();
    }
}
