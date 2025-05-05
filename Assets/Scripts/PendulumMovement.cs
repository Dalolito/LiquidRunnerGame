using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumMovement : MonoBehaviour 
{
    [Header("Pendulum Settings")]
    public float swingAmplitude = 45f;     // Amplitud en grados
    public float swingSpeed = 2f;          // Velocidad de oscilación constante
    public float phaseOffset = 0f;         // Desplazamiento de fase (0-360)
    
    [Header("Appearance")]
    public bool preserveOriginalScale = true; // Mantener la escala original
    
    private float timeCounter = 0f;
    private Quaternion startRotation;
    private Vector3 originalScale;

    void Start()
    {
        // Guardar la rotación inicial
        startRotation = transform.rotation;
        
        // Guardar la escala original
        originalScale = transform.localScale;
        
        // Aplicar el desplazamiento de fase (convertir a radianes)
        timeCounter = phaseOffset * Mathf.Deg2Rad;
    }

    void Update()
    {
        // Incrementar el contador de tiempo con una velocidad constante
        timeCounter += Time.deltaTime * swingSpeed;
        
        // Calcular el ángulo de rotación basado en una función seno
        float angle = Mathf.Sin(timeCounter) * swingAmplitude;
        
        // Aplicar la rotación alrededor del eje Z
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        // Asegurarse de que la escala se mantenga siempre igual a la original
        if (preserveOriginalScale && transform.localScale != originalScale)
        {
            transform.localScale = originalScale;
        }
    }
}