using UnityEngine;

public class Enemy : MonoBehaviour
{
    public BoomEffect boomEffectPrefab; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject); 
            Destroy(gameObject);  
            FindObjectOfType<SpaceInvadersManager>().EnemyKilled();
            Vector3 centerPos = new Vector3(0, 0, 0);
            BoomEffect boom = Instantiate(boomEffectPrefab, centerPos, Quaternion.identity);
            boom.Play();
        }
    }
}