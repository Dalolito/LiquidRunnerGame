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
            Debug.Log("Player hit by lava wall!");
            
            // Obtener el controlador del jugador
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