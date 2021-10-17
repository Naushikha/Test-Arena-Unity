using UnityEngine;

public class Target : MonoBehaviour
{
    public void getHit(float damage, RaycastHit hit)
    {
        hitData tmpData = new hitData(damage, hit);
        // For this to work, the gameObject this is attached to should have implemented a takeDamage function
        gameObject.SendMessage("takeDamage", tmpData); 
    }
}

// Data structure for sending hit info
public class hitData
{
    public float damage;
    public RaycastHit hit;
    public hitData(float damage, RaycastHit hit)
    {
        this.damage = damage;
        this.hit = hit;
    }
}