using UnityEngine;

public class Enemy : MonoBehaviour
{
    public BoomEffect boomEffectPrefab;
    public Transform SpawnBoom;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject); 
            Destroy(gameObject);  
            var mgr = FindAnyObjectByType<SpaceInvadersManager>();
            if (mgr != null)
            {
                mgr.EnemyKilled();
            }
            if (boomEffectPrefab != null)
            {
                Vector3 boomPos = SpawnBoom != null ? SpawnBoom.position : transform.position;
                BoomEffect boom = Instantiate(boomEffectPrefab, boomPos, Quaternion.identity);
                if (boom != null) boom.Play();
            }
        }
    }
}