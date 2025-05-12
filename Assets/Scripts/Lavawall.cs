using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaWall : MonoBehaviour
{
    [Header("Wall Properties")]
    public bool isDeadly = true;
    public float damage = 1f;
    
    private void Start()
    {
        // Asegurarse de que el objeto tiene el tag correcto
        gameObject.tag = "Obstacle";
        
        // Verificar si tiene collider y configurarlo como trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
            Debug.Log("LavaWall initialized with collider as trigger");
        }
        else
        {
            Debug.LogWarning("LavaWall has no collider attached: " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Mensaje de depuración para todas las colisiones
        Debug.Log("Collision detected with " + other.gameObject.name + " (tag: " + other.tag + ")");
        
        // Si el jugador choca con este muro
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by lava wall!");
            
            // Intentar obtener el controlador desde el objeto raíz del jugador
            LiquidCharacterController playerController = other.GetComponent<LiquidCharacterController>();
            if (playerController == null)
            {
                playerController = other.GetComponentInParent<LiquidCharacterController>();
            }
            
            if (playerController != null)
            {
                if (isDeadly)
                {
                    playerController.Die();
                }
                else
                {
                    playerController.TakeDamage(damage);
                }
            }
            else
            {
                Debug.LogError("Player controller not found! Calling GameOver directly");
                
                // Llamar directamente al GameManager como respaldo
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.GameOver();
                }
            }
        }
    }
    
    // Método adicional para manejar colisiones no trigger
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Regular collision with " + collision.gameObject.name);
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with lava wall!");
            
            // Intentar obtener el controlador del jugador
            LiquidCharacterController playerController = collision.gameObject.GetComponent<LiquidCharacterController>();
            if (playerController == null)
            {
                playerController = collision.gameObject.GetComponentInParent<LiquidCharacterController>();
            }
            
            if (playerController != null)
            {
                if (isDeadly)
                {
                    playerController.Die();
                }
                else
                {
                    playerController.TakeDamage(damage);
                }
            }
            else
            {
                // Llamar directamente al GameManager
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.GameOver();
                }
            }
        }
    }
}