using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarperSFX : MonoBehaviour
{
    public int shotSFXNumber = 3;
    public void flinch()
    {
        transform.Find("SFX/flinch").GetComponent<AudioSource>().Play();
    }
    public void attack()
    {
        transform.Find("SFX/attack").GetComponent<AudioSource>().Play();
    }
    public void seen()
    {
        transform.Find("SFX/seen").GetComponent<AudioSource>().Play();
    }
    public void die()
    {
        transform.Find("SFX/die").GetComponent<AudioSource>().Play();
    }
    public void shot()
    {
        transform.Find($"SFX/hit{Random.Range(1, shotSFXNumber) + 1}").GetComponent<AudioSource>().Play();
    }
}
