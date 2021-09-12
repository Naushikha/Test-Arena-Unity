using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float playerHealth = 100f;
    public int currentWave = 0;
    public GameObject alienPrefab;
    public Text txtHealth;
    public Text txtAmmo;
    public Text txtWave;
    public Text txtAliens;
    public AudioSource sfxWave;
    public GameObject noEscape;
    public GameObject hud_blood;

    public Material skybox_default;
    public Material skybox_virtual;

    private List<GameObject> prevAliens = new List<GameObject>();
    private int killedAliens = 0;
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
        txtHealth.text = "HEALTH | " + playerHealth;
        startNextWave();
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
        txtHealth.text = "HEALTH | " + playerHealth;
        // StartCoroutine("Fade");
    }
    public void alienKilled()
    {
        killedAliens++;
        if (killedAliens == prevAliens.Count)
        {
            startNextWave();
        }
    }
    void startNextWave()
    {
        currentWave++;
        txtWave.text = "Wave" + currentWave;
        txtWave.gameObject.SetActive(true);
        sfxWave.Play();
        // Destroy all previous aliens
        if (prevAliens != null)
        {
            foreach (GameObject alien in prevAliens)
            {
                Destroy(alien);
            }
        }
        killedAliens = 0;
        prevAliens.Clear();
        int aliensToGen = (int)Mathf.Pow(2, currentWave);
        while (aliensToGen > 0)
        {
            int randX = Random.Range(25, 95);
            int randZ = Random.Range(5, 95);
            GameObject newAlien = Instantiate(alienPrefab, new Vector3(randX, 0, randZ), Quaternion.identity) as GameObject;
            int randRotY = Random.Range(0, 360);
            newAlien.transform.Rotate(0, randRotY, 0);
            prevAliens.Add(newAlien);
            aliensToGen--;
        }
        txtAliens.text = "LEFT | " + prevAliens.Count;
        StartCoroutine(hideWave());
    }
    IEnumerator hideWave()
    {
        RenderSettings.skybox = skybox_virtual;
        yield return new WaitForSeconds(3);
        for (float f = 1f; f >= -0.05f; f -= 0.05f)
        {
            Color tmp = txtWave.color;
            tmp.a = f;
            txtWave.color = tmp;
            yield return new WaitForSeconds(0.1f);
        }
        RenderSettings.skybox = skybox_default;
        txtWave.gameObject.SetActive(false);
        Color tmpReset = txtWave.color;
        tmpReset = txtWave.color;
        tmpReset.a = 1f;
        txtWave.color = tmpReset;
    }
}
