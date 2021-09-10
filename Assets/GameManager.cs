using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float playerHealth = 100f;

    public GameObject healthText;

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
    void Update()
    {
        if (playerHealth <= 0)
        {
            // Debug.Log("PLAYER IS DEAD!");
        }
    }

    public void playerHurt(float amount)
    {
        playerHealth -= amount;
        Debug.Log("Player hurt!");
        Debug.Log(playerHealth);
        Text tmp = healthText.GetComponent<Text>();
        tmp.text = "Health left: " + playerHealth;
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
