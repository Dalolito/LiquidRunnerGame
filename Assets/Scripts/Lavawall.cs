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
                // Aquí podrías llamar a un método de daño o muerte en el controlador del jugador
                // Por ejemplo: playerController.TakeDamage(damage);
            }
        }
    }
}