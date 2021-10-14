// https://www.youtube.com/watch?v=hifCUD3dATs
// https://www.youtube.com/watch?v=Ap8aTjv72mI
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public float amount = 0.03f;
    public float maxAmount = 1.0f;
    public float smoothAmount = 6f;
    private Vector3 initialPos;
    // Weapon bobbing
    public float standIntensity = 0.025f;
    public float walkIntensity = 0.03f;
    public float runIntensity = 0.05f;
    public enum bobState { Idle, Stand, Walk, Run };
    public bobState currentBobState = bobState.Stand;
    private float bobCounter = 0f;
    void Start()
    {
        initialPos = transform.localPosition;
    }
    void LateUpdate()
    {
        float movementX = -Input.GetAxis("Mouse X") * amount;
        float movementY = -Input.GetAxis("Mouse Y") * amount;
        movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
        movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);
        Vector3 finalPos = new Vector3(movementX, movementY, 0);
        // Add in weapon bobbing
        finalPos += bobHead();
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + initialPos, Time.deltaTime * smoothAmount);
    }

    Vector3 bobHead()
    {
        // Get correct intensity
        float currentIntensity;
        switch (currentBobState)
        {
            case bobState.Stand:
                currentIntensity = standIntensity;
                bobCounter += Time.deltaTime;
                break;
            case bobState.Walk:
                currentIntensity = walkIntensity;
                bobCounter += Time.deltaTime * 4;
                break;
            case bobState.Run:
                currentIntensity = runIntensity;
                bobCounter += Time.deltaTime * 6;
                break;
            case bobState.Idle:
            default:
                currentIntensity = 0;
                break;
        }
        // Check for overflow
        if (bobCounter * Mathf.Deg2Rad >= 360) bobCounter = 0;
        return new Vector3(Mathf.Cos(bobCounter) * currentIntensity, Mathf.Sin(bobCounter * 2) * currentIntensity, 0);
    }
}
