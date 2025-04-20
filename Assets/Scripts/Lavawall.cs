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
        // Si el jugador choca con este muro
        if (other.CompareTag("Player"))
        {
            // Aquí puedes implementar la lógica de muerte o daño
            Debug.Log("Player hit by lava wall!");
            
            // Ejemplo de cómo interactuar con el controlador del personaje
            LiquidCharacterController playerController = other.GetComponent<LiquidCharacterController>();
            if (playerController != null)
            {
                // Verificar si el obstáculo es mortal
                if (isDeadly)
                {
                    // Si es mortal, causar muerte inmediata
                    playerController.Die();
                }
                else
                {
                    // Si no es mortal, aplicar daño según la cantidad configurada
                    playerController.TakeDamage(damage);
                }
            }
        }
    }
}