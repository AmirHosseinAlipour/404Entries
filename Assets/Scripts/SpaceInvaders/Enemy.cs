using UnityEngine;

public class Enemy : MonoBehaviour
{
    public BoomEffect boomEffectPrefab;
    public Transform SpawnBoom;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject); 
            Destroy(gameObject);  
            FindObjectOfType<SpaceInvadersManager>().EnemyKilled();
            Vector3 centerPos = new Vector3(0, 0, 0);
            BoomEffect boom = Instantiate(boomEffectPrefab, SpawnBoom.position, Quaternion.identity);
            boom.Play();
        }
    }
}