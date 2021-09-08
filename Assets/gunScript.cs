using UnityEngine;

public class gunScript : MonoBehaviour
{
    public Camera fpsCam;
    public float damage = 10f;
    public float range = 100f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target target = hit.transform.root.gameObject.GetComponent<Target>();

            if (target != null)
            {
                target.takeDamage(damage);
            }

        }

    }
}
