using UnityEngine;

public class gunScript : MonoBehaviour
{
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;

    public int NumOfShootAnimations = 2;

    // SFX
    public AudioSource SFX_fire;

    private float nextTimeToFire = 0f;

    private Animator animator;

    // Update is called once per frame

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            SFX_fire.pitch = Random.Range(0.9f, 1.1f);
            SFX_fire.Play();
            Shoot();
        }
    }

    void Shoot()
    {
        animator.Play("base.shoot" + Random.Range(1, NumOfShootAnimations + 1), 0, 0);
        // Play sound here
        muzzleFlash.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            // Debug.Log(hit.transform.name);

            Intelligence target = hit.transform.root.gameObject.GetComponent<Intelligence>();

            if (target != null)
            {
                target.takeDamage(damage);
            }

        }

    }
}
