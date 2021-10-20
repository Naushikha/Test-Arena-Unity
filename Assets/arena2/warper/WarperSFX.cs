using UnityEngine;

public class WarperSFX : MonoBehaviour
{
    public AudioSource sfxBreathe, sfxFlinch, sfxAttack, sfxSeen, sfxDie, sfxStabbed;
    public AudioSource[] sfxHit;
    public void breathe() { if (!sfxBreathe.isPlaying) sfxBreathe.Play(); }
    public void flinch() { sfxFlinch.Play(); }
    public void attack() { sfxAttack.Play(); }
    public void seen() { sfxSeen.Play(); }
    public void die() { sfxDie.Play(); }
    public void shot() { sfxHit[Random.Range(1, sfxHit.Length)].Play(); }
    public void stabbed() { sfxStabbed.Play(); }
}
