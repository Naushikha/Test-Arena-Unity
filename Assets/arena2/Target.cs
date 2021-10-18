using UnityEngine;

public class Target : MonoBehaviour
{
    public void getHit(float damage, RaycastHit hit, hitType type = hitType.gun)
    {
        hitData tmpData = new hitData(damage, hit, type);
        // For this to work, the gameObject this is attached to should have implemented a takeDamage function
        gameObject.SendMessage("takeDamage", tmpData);
    }
}

// Data structure for sending hit info
public class hitData
{
    public float damage;
    public RaycastHit hit;
    public hitType type;
    public hitData(float damage, RaycastHit hit, hitType type)
    {
        this.damage = damage;
        this.hit = hit;
        this.type = type;
    }
}

public enum hitType
{
    gun,
    knife
}