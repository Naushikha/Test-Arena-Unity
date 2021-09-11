using UnityEngine;
using System.Collections;

public class knifeScript : MonoBehaviour
{
    public Camera fpsCam;
    public float damage = 70f;
    public float range = 1f;
    public float fireRate = 1f;

    // SFX
    public AudioSource SFX_fire_hit;
    public AudioSource SFX_fire_none;
    public AudioSource SFX_draw;

    private float nextTimeToFire = 0f;
    private bool isDrawing = false;
    private bool initialized = false;
    private Animator animator;

    // Update is called once per frame

    private void Start()
    {
        animator = GetComponent<Animator>();
        displayHUDAmmo();
        StartCoroutine(draw());
        initialized = true;
    }
    void Update()
    {
        if (isDrawing) return;
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }
    void Shoot()
    {
        animator.Play("base.shoot1", 0, 0);
        SFX_fire_none.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            // Debug.Log(hit.transform.name);

            Intelligence target = hit.transform.root.gameObject.GetComponent<Intelligence>();

            if (target != null)
            {
                SFX_fire_hit.Play();
                target.takeDamage(damage);
                // Set damage according to how severe it is
            }
        }

    }
    IEnumerator draw()
    {
        isDrawing = true;
        SFX_draw.Play();
        animator.Play("base.draw", 0, 0);
        while (!(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1))
        {
            yield return null;
        }
        isDrawing = false;
    }
    void OnEnable()
    {
        if (initialized) // Do not want calling NULL objects
        {
            displayHUDAmmo();
            StartCoroutine(draw());
        }
    }

    void displayHUDAmmo()
    {
        GameManager.Instance.txtAmmo.text = "/̵͇̿̿/'̿̿ AMMO: ∞";
    }
}
