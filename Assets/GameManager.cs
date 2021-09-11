using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float playerHealth = 100f;

    public Text txtHealth;
    public Text txtAmmo;
    public GameObject noEscape;
    public GameObject hud_blood;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        txtHealth.text = "❤️ HEALTH: " + playerHealth;
    }
    void Update()
    {
        if (playerHealth <= 0)
        {
            // Debug.Log("PLAYER IS DEAD!");
        }
    }

    public void playerHurt(float amount)
    {
        if (!hud_blood.activeSelf) hud_blood.SetActive(true);
        playerHealth -= amount;
        txtHealth.text = "❤️ HEALTH: " + playerHealth;
        // StartCoroutine("Fade");
    }
    // IEnumerator Fade()
    // {
    //     Image yo = canvas.GetComponent<Image>();
    //     for (float ft = 1f; ft >= 0; ft -= 0.1f)
    //     {
    //         Color c = yo.color;
    //         c.a = ft;
    //         yo.material.color = c;
    //         yield return null;
    //     }
    // }
}
