using UnityEngine;

public class theresNoEscape : MonoBehaviour
{
    public float terrainSize = 100f; // Length and Width

    public float warningOffset = 5f;
    private float valMin;
    private float valMax;
    // Start is called before the first frame update
    void Start()
    {
        valMin = warningOffset;
        valMax = terrainSize - warningOffset;
        Debug.Log(valMin);
        Debug.Log(valMax);

    }

    // Update is called once per frame
    void Update()

    {
        if (transform.position.x < valMin || transform.position.x > valMax || transform.position.z < valMin || transform.position.z > valMax)
        {
            GameManager.Instance.noEscape.SetActive(true);
        }
        else
        {
            // Debug.Log(transform.position);
            GameManager.Instance.noEscape.SetActive(false);
        }
    }
}
