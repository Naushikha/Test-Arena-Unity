using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Panther;
using Goliath;
using Warper;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public bool legacyArena = true;
    public int arenaSize = 100;
    public float playerHealth = 100f;
    public int currentLevel = 0;
    public GameObject[] alienPrefabs;
    public Transform weapons;
    public GameObject player;
    public Text txtHealth;
    public Text txtAmmo;
    public Text txtLevel;
    public Text txtAliens;
    public Color blinkColor1;
    public Color blinkColor2;
    public AudioSource sfxLevel;
    public GameObject noEscape;
    public GameObject hud_blood;

    public Material skybox_default;
    public Material skybox_virtual;

    private List<GameObject> prevAliens = new List<GameObject>();
    private int killedAliens = 0;

    public Text txtWarning;
    public string[] warningList;
    private bool warningShown = false;

    public GameObject gameMusic;
    private int currentMusic = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // I want this removed going back to menu
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        txtHealth.text = "HEALTH | " + playerHealth;
        if (legacyArena) startNextLevel();
        else { startNextLevel_Arena2(); StartCoroutine(pollRandomTriggers()); }
    }
    public void playerHurt(float amount)
    {
        if (!hud_blood.activeSelf) hud_blood.SetActive(true);
        playerHealth -= amount;
        txtHealth.text = "HEALTH | " + playerHealth;
        if (playerHealth <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.currentLevel = currentLevel - 1;
            GameManager.Instance.score = Mathf.FloorToInt(GameManager.Instance.aliensKilled * 10 * GameManager.Instance.difficulty * (1 + (currentLevel / 6)));
            SceneManager.LoadScene("score");
        }
    }
    public void alienKilled()
    {
        StartCoroutine(blinkLeft());
        killedAliens++;
        if (GameManager.Instance) GameManager.Instance.aliensKilled++; // Increase aliens for score
        txtAliens.text = "(:|:) LEFT | " + (prevAliens.Count - killedAliens);
        if (killedAliens == prevAliens.Count)
        {
            // Refill ammo
            foreach (Transform weapon in weapons)
            {
                gunScript target = weapon.gameObject.GetComponent<gunScript>();
                if (target != null)
                {
                    target.resetAmmo();
                }
            }
            if (legacyArena) startNextLevel();
            else startNextLevel_Arena2();
        }
    }
    void startNextLevel()
    {
        currentLevel++;
        txtLevel.text = "LEVEL " + currentLevel;
        txtLevel.gameObject.SetActive(true);
        sfxLevel.Play();
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
        int aliensToGen = (int)Mathf.Pow(2, currentLevel);
        if (alienPrefabs[0])
        {
            while (aliensToGen > 0)
            {
                int randX = Random.Range(25, 95);
                int randZ = Random.Range(5, 95);
                GameObject newAlien = Instantiate(alienPrefabs[0], new Vector3(randX, 0, randZ), Quaternion.identity) as GameObject;
                int randRotY = Random.Range(0, 360);
                newAlien.transform.Rotate(0, randRotY, 0);
                prevAliens.Add(newAlien);
                aliensToGen--;
            }

        }
        txtAliens.text = "(:|:) LEFT | " + prevAliens.Count;
        Vector3 tmpPosition = new Vector3(16.2099991f, 7.15999985f, 5.38999987f);
        player.transform.position = tmpPosition;
        player.transform.eulerAngles = Vector3.zero;
        StartCoroutine(hideLevel());
        playNextMusic();
    }
    void startNextLevel_Arena2()
    {
        currentLevel++;
        txtLevel.text = "LEVEL " + currentLevel;
        txtLevel.gameObject.SetActive(true);
        sfxLevel.Play();
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
        int aliensToGen = (int)Mathf.Pow(2, currentLevel);
        if (alienPrefabs[0])
        {
            while (aliensToGen > 0)
            {
                int randX = Random.Range(25, arenaSize - 5);
                int randZ = Random.Range(5, arenaSize - 5);
                GameObject newAlien = Instantiate(alienPrefabs[Random.Range(0, alienPrefabs.Length)], new Vector3(randX, 0, randZ), Quaternion.identity) as GameObject;
                int randRotY = Random.Range(0, 360);
                newAlien.transform.Rotate(0, randRotY, 0);
                prevAliens.Add(newAlien);
                aliensToGen--;
            }

        }
        txtAliens.text = "(:|:) LEFT | " + prevAliens.Count;
        Vector3 tmpPosition = new Vector3(16.2099991f, 7.15999985f, 5.38999987f);
        player.transform.position = tmpPosition;
        player.transform.eulerAngles = Vector3.zero;
        StartCoroutine(hideLevel());
        playNextMusic();
    }
    IEnumerator hideLevel()
    {
        RenderSettings.skybox = skybox_virtual;
        yield return new WaitForSeconds(3);
        for (float f = 1f; f >= -0.05f; f -= 0.05f)
        {
            Color tmp = txtLevel.color;
            tmp.a = f;
            txtLevel.color = tmp;
            yield return new WaitForSeconds(0.1f);
        }
        RenderSettings.skybox = skybox_default;
        txtLevel.gameObject.SetActive(false);
        Color tmpReset = txtLevel.color;
        tmpReset = txtLevel.color;
        tmpReset.a = 1f;
        txtLevel.color = tmpReset;
    }

    public void showWarning()
    {
        if (!warningShown)
        {
            txtWarning.text = warningList[Random.Range(0, warningList.Length)];
            txtWarning.gameObject.SetActive(true);
            warningShown = true;
            StartCoroutine(hideWarning());
        }
    }
    IEnumerator hideWarning()
    {
        for (float f = 0f; f <= 1f; f += 0.05f)
        {
            Color tmp = txtWarning.color;
            tmp.a = f;
            txtWarning.color = tmp;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1f);
        for (float f = 1f; f >= -0.05f; f -= 0.05f)
        {
            Color tmp = txtWarning.color;
            tmp.a = f;
            txtWarning.color = tmp;
            yield return new WaitForSeconds(0.01f);
        }
        txtWarning.gameObject.SetActive(false);
        Color tmpReset = txtWarning.color;
        tmpReset = txtWarning.color;
        tmpReset.a = 1f;
        txtWarning.color = tmpReset;
        yield return new WaitForSeconds(0.5f);
        warningShown = false;
    }

    void playNextMusic()
    {
        gameMusic.transform.GetChild(currentMusic).GetComponent<AudioSource>().Stop();
        currentMusic++;
        if (gameMusic.transform.childCount - 1 == currentMusic)
        {
            currentMusic = 0;
        }
        gameMusic.transform.GetChild(currentMusic).GetComponent<AudioSource>().Play();
    }

    IEnumerator blinkLeft()
    {
        for (int i = 0; i < 7; i++)
        {
            yield return new WaitForSeconds(0.2f);
            txtAliens.color = blinkColor2;
            yield return new WaitForSeconds(0.2f);
            txtAliens.color = blinkColor1;
        }
    }

    IEnumerator pollRandomTriggers()
    {
        yield return new WaitForSeconds(30);
        makeRandomMonsterChasePlayer();
        StartCoroutine(pollRandomTriggers());
    }

    void makeRandomMonsterChasePlayer()
    {
        for (int i = 0; i < prevAliens.Count; i++)
        {
            PantherAI panther = prevAliens[i].GetComponent<PantherAI>();
            if (panther && !panther.isDead()) { panther.sendAfterPlayer(); break; }
            GoliathAI goliath = prevAliens[i].GetComponent<GoliathAI>();
            if (goliath && !goliath.isDead()) { goliath.sendAfterPlayer(); break; }
            WarperAI warper = prevAliens[i].GetComponent<WarperAI>();
            if (warper && !warper.isDead()) { warper.sendAfterPlayer(); break; }
        }
    }
}
