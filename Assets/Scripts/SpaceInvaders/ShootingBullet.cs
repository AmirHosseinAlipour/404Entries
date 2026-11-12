using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Random = UnityEngine.Random;

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
    private SoundPlayer soundPlayer;

    private void Awake()
    {
        soundPlayer = GetComponent<SoundPlayer>();
    }

    private void Start()
    {
        if (shootButton != null)
            shootButton.onClick.AddListener(() => TryShoot());
    }

    private void Update()
    {
        bool shootInput = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                UnityEngine.InputSystem.Keyboard.current.upArrowKey.wasPressedThisFrame ||
                UnityEngine.InputSystem.Keyboard.current.wKey.wasPressedThisFrame ||
                UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                shootInput = true;
            }
        }
        if (!shootInput && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Return)))
        {
            shootInput = true;
        }

        if (shootInput)
        {
            TryShoot();
        }
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
        soundPlayer.Play("Shoot");
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
