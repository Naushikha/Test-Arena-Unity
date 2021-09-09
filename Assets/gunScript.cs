using UnityEngine;

public class gunScript : MonoBehaviour
{
    public Camera fpsCam;

    public ParticleSystem muzzleFlash;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;

    private float nextTimeToFire = 0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
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
