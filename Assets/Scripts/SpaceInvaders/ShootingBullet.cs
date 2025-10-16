using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShootingBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject[] bulletPrefabs;   
    public Transform shootPoint;       
    public float shootPower = 10f;       

    [Header("Cooldown Settings")]
    public float shootCooldown = 0.5f; 
    private bool canShoot = true;

    [Header("Optional UI")]
    public Button shootButton; 

    private void Start()
    {
        if (shootButton != null)
            shootButton.onClick.AddListener(() => TryShoot());
    }
    private void TryShoot()
    {
        if (canShoot)
            StartCoroutine(ShootRandomBullet());
    }

    private IEnumerator ShootRandomBullet()
    {
        canShoot = false;
        if (bulletPrefabs.Length == 0 || shootPoint == null)
        {
            Debug.LogWarning("Missing bulletPrefabs or shootPoint!");
            yield break;
        }
        int randomIndex = Random.Range(0, bulletPrefabs.Length);
        GameObject chosenBullet = bulletPrefabs[randomIndex];
        GameObject bulletInstance = Instantiate(chosenBullet, shootPoint.position, chosenBullet.transform.rotation);
        Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * shootPower;
        }
        Destroy(bulletInstance, 5f);
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
}
