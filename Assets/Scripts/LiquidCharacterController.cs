using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    
    [Header("Health Settings")]
    public float maxHealth = 3f;
    private float currentHealth;
    
    public enum CharacterShape { Sphere, Cube, Flat }

    [Header("Shape Changing")]
    public CharacterShape currentShape = CharacterShape.Sphere;
    
    // References to different shape meshes/objects
    public GameObject sphereShape;
    public GameObject cubeShape;
    public GameObject flatShape;
    
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isDead = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        UpdateCharacterShape();
    }
    
    void Update()
    {
        // No procesar inputs si el personaje está muerto
        if (isDead) return;
        
        // Handle movement
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 movement = new Vector3(horizontalInput, 0, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
        
        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        
        // Handle shape changing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeShape(CharacterShape.Sphere);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeShape(CharacterShape.Cube);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeShape(CharacterShape.Flat);
        }
    }
    
    void ChangeShape(CharacterShape newShape)
    {
        currentShape = newShape;
        UpdateCharacterShape();
    }
    
    void UpdateCharacterShape()
    {
        // Disable all shapes first
        sphereShape.SetActive(false);
        cubeShape.SetActive(false);
        flatShape.SetActive(false);
        
        // Enable the current shape
        switch (currentShape)
        {
            case CharacterShape.Sphere:
                sphereShape.SetActive(true);
                break;
            case CharacterShape.Cube:
                cubeShape.SetActive(true);
                break;
            case CharacterShape.Flat:
                flatShape.SetActive(true);
                break;
        }
        
        // Update the collider to match the new shape
        UpdateCollider();
    }
    
    void UpdateCollider()
    {
        // Remove existing colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
        {
            Destroy(c);
        }
        
        // Add the appropriate collider based on the shape
        switch (currentShape)
        {
            case CharacterShape.Sphere:
                SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
                sphereCol.radius = 1.25f;
                sphereCol.center = new Vector3(0, -0.1f, 0); // Ajusta el centro ligeramente hacia abajo
                break;
                
            case CharacterShape.Cube:
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.center = new Vector3(0, 1, -1); // Centro coincide con la posición
                boxCol.size = new Vector3(0.7f, 4f, 1f); // Mismo tamaño que la escala
                break;
                
            case CharacterShape.Flat:
                BoxCollider flatCol = gameObject.AddComponent<BoxCollider>();
                flatCol.center = new Vector3(0, 0, -0.8f); // Centro coincide con la posición
                flatCol.size = new Vector3(6f, 2f, 1f); // Mismo tamaño que la escala
                break;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if we've landed on something
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
    
    // Método para recibir daño
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        
        currentHealth -= damageAmount;
        Debug.Log("Player took damage! Current Health: " + currentHealth);
        
        // Verificar si el jugador ha muerto
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Método para manejar la muerte del personaje
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Player has died!");
        
        // Desactivar la física del personaje
        rb.isKinematic = true;
        
        // Deshabilitar los colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }
        
        // Puedes añadir efectos visuales de muerte aquí
        
        // Opcionalmente, reiniciar el nivel después de un tiempo
        // Invoke("RestartLevel", 2f);
    }
    
    // Método para reiniciar el nivel
    void RestartLevel()
    {
        // Cargar la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}