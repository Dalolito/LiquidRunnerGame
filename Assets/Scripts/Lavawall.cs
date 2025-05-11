using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaWall : MonoBehaviour
{
    [Header("Wall Properties")]
    public bool isDeadly = true;
    public float damage = 1f;

    private void OnTriggerEnter(Collider other)
    {
        // Mensaje de depuración para todas las colisiones
        Debug.Log("Collision detected with " + other.gameObject.name + " (tag: " + other.tag + ")");
        
        // Si el jugador choca con este muro
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by lava wall!");
            
            // Obtener el controlador del jugador
            LiquidCharacterController playerController = other.GetComponent<LiquidCharacterController>();
            
            // Si no encontramos el controlador en el objeto directo, buscamos en el padre
            if (playerController == null)
            {
                playerController = other.GetComponentInParent<LiquidCharacterController>();
                Debug.Log("Searching for player controller in parent: " + (playerController != null ? "Found" : "Not found"));
            }
            
            // Intentar obtener el controlador desde el objeto raíz del jugador
            if (playerController == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerController = player.GetComponent<LiquidCharacterController>();
                    Debug.Log("Trying to find player controller by tag: " + (playerController != null ? "Found" : "Not found"));
                }
            }
            
            if (playerController != null)
            {
                // Verificar si el obstáculo es mortal
                if (isDeadly)
                {
                    // Si es mortal, causar muerte inmediata
                    Debug.Log("Calling Die() on player controller");
                    playerController.Die();
                }
                else
                {
                    // Si no es mortal, aplicar daño según la cantidad configurada
                    Debug.Log("Applying damage to player: " + damage);
                    playerController.TakeDamage(damage);
                }
            }
            else
            {
                Debug.LogError("Player controller component not found! Attempting to trigger game over directly.");
                
                // Intento de recuperación: llamar directamente al GameManager
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
        Debug.Log("Regular collision with " + collision.gameObject.name + " (tag: " + collision.gameObject.tag + ")");
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with lava wall (non-trigger)!");
            
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
                // Intento de recuperación: llamar directamente al GameManager
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.GameOver();
                }
            }
        }
    }
}