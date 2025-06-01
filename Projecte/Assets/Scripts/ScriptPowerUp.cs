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

        SetupAppearance();

        StartCoroutine(MoveDown());
    }

    private void SetupAppearance()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.PowerBall:
                    renderer.material.color = Color.red;
                    break;
                case PowerUpType.BigPaddle:
                    renderer.material.color = Color.green;
                    break;
                case PowerUpType.SmallPaddle:
                    renderer.material.color = Color.blue;
                    break;
                case PowerUpType.NormalBall:
                    renderer.material.color = Color.cyan;
                    break;
                case PowerUpType.Magnet:
                    renderer.material.color = Color.yellow;
                    break;
                case PowerUpType.ExtraLife:
                    renderer.material.color = Color.magenta;
                    break;
                case PowerUpType.NextLevel:
                    renderer.material.color = Color.black;
                    break;
                case PowerUpType.GodMode:
                    renderer.material.color = new Color(1f, 0.5f, 0f); // Naranja o Dorado
                    break;
                case PowerUpType.Rocket:
                    renderer.material.color = new Color(0.8f, 0.4f, 0f); // Color marr�n/naranja oscuro
                    break;
            }
        }

        TextMesh textMesh = GetComponentInChildren<TextMesh>();
        if (textMesh != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.PowerBall:
                    textMesh.text = "P";
                    break;
                case PowerUpType.BigPaddle:
                    textMesh.text = "B";
                    break;
                case PowerUpType.SmallPaddle:
                    textMesh.text = "S";
                    break;
                case PowerUpType.NormalBall:
                    textMesh.text = "N";
                    break;
                case PowerUpType.Magnet:
                    textMesh.text = "M";
                    break;
                case PowerUpType.ExtraLife:
                    textMesh.text = "+1";
                    break;
                case PowerUpType.NextLevel:
                    textMesh.text = "NL";
                    break;
                case PowerUpType.GodMode: // �Nuevo caso!
                    textMesh.text = "GM"; // Texto para God Mode
                    break;
                case PowerUpType.Rocket:
                    textMesh.text = "R"; // Texto para Rocket
                    break;
            }
        }
    }

    private IEnumerator MoveDown()
    {
        while (transform.position.z > -10f) // L�mite inferior
        {
            transform.Translate(Vector3.back * fallSpeed * Time.deltaTime);
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
    }
}