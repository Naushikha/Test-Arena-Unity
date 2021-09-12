using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class gunScript : MonoBehaviour
{
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public int magazines = 5;
    public int ammoInMag = 20;
    private int currentAmmo;
    private int currentMags;
    public int NumOfShootAnimations = 2;

    public Image crosshair;

    // SFX
    public AudioSource SFX_fire;

    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private bool isDrawing = false;
    private bool initialized = false;
    private Animator animator;

    // Update is called once per frame

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentAmmo = ammoInMag;
        currentMags = --magazines;
        displayHUDAmmo();
        StartCoroutine(waitTillDraw());
        initialized = true;
    }
    void Update()
    {
        if (isDrawing) return;
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R) && ammoInMag != currentAmmo)
        {
            StartCoroutine(Reload());
        }
    }
    void Shoot()
    {
        currentAmmo--;
        displayHUDAmmo();
        animator.Play("base.shoot" + Random.Range(1, NumOfShootAnimations + 1), 0, 0);
        // Play sound here
        SFX_fire.pitch = Random.Range(0.95f, 1.05f);
        SFX_fire.Play();
        muzzleFlash.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            // Debug.Log(hit.transform.name);

            Intelligence target = hit.transform.root.gameObject.GetComponent<Intelligence>();

            if (target != null)
            {
                target.takeDamage(damage);
                // Set damage according to how severe it is
            }

        }

    }
    IEnumerator Reload()
    {
        if (currentMags == 0) yield break;
        isReloading = true;
        crosshairVisible(0); // make crosshair go away
        animator.Play("base.reload", 0, 0);
        yield return new WaitForSeconds(0.001f); // Add a tiny delay to reset animator?
        while (!(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)) // Wait until animation is over
        {
            yield return null;
        }
        currentAmmo = ammoInMag;
        currentMags--;
        displayHUDAmmo();
        crosshairVisible(1); // put crosshair back
        isReloading = false;
    }
    IEnumerator waitTillDraw()
    {
        isDrawing = true;
        crosshairVisible(0); // make crosshair go away
        animator.Play("base.draw", 0, 0);
        while (!(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1))
        {
            yield return null;
        }
        crosshairVisible(1); // put crosshair back
        isDrawing = false;
        if (isReloading) // Switched previously in the middle of a reload?
        {
            StartCoroutine(Reload());
        }
    }
    void OnEnable()
    {
        if (initialized) // Do not want calling NULL objects
        {
            displayHUDAmmo();
            StartCoroutine(waitTillDraw());
        }
    }

    void displayHUDAmmo()
    {
        GameManager.Instance.txtAmmo.text = "AMMO | " + currentMags + " / " + currentAmmo;
    }

    void crosshairVisible(int alpha)
    {
        Color tmp = crosshair.color;
        tmp.a = alpha;
        crosshair.color = tmp;
    }
}
