using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    private PowerUpType powerUpType;
    private float fallSpeed;
    private Rigidbody rb;

    public void Initialize(PowerUpType type, float speed)
    {
        powerUpType = type;
        fallSpeed = speed;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;

        StartCoroutine(MoveDown());
    }

    private IEnumerator MoveDown()
    {
        while (transform.position.z > -10f) // L�mite inferior
        {
            transform.Translate(Vector3.back * fallSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Paleta"))
        {
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivatePowerUp(powerUpType);
            }
            Destroy(gameObject);
        }
        else return;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Paleta"))
        {
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.ActivatePowerUp(powerUpType);
            }
            Destroy(gameObject);
        }
        else return;
    }
}