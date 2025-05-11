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
    
    public enum CharacterShape { Cube, Tetris1, Tetris2, Tetris3 }

    [Header("Shape Changing")]
    public CharacterShape currentShape = CharacterShape.Cube;
    
    // References to different shape meshes/objects
    public GameObject cubeShape;
    public GameObject tetrisFigure1;
    public GameObject tetrisFigure2;
    public GameObject tetrisFigure3;
    
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isDead = false;
    
    [Header("Height Adjustment")]
    public float characterHeight = 2f; // Altura base del personaje sobre el suelo
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        
        // Asegurarse de que el Rigidbody esté configurado correctamente
        if (rb != null)
        {
            rb.mass = 1.0f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.freezeRotation = true; // Evita que el personaje gire
        }
        
        // Centrar todas las figuras
        CenterAllFigures();
        
        // Elevar el personaje completo
        transform.position = new Vector3(transform.position.x, characterHeight, transform.position.z);
        
        UpdateCharacterShape();
    }
    
    void CenterAllFigures()
    {
        // Centrar todas las figuras en el origen del personaje
        if (cubeShape != null) cubeShape.transform.localPosition = Vector3.zero;
        if (tetrisFigure1 != null) tetrisFigure1.transform.localPosition = Vector3.zero;
        if (tetrisFigure2 != null) tetrisFigure2.transform.localPosition = Vector3.zero;
        if (tetrisFigure3 != null) tetrisFigure3.transform.localPosition = Vector3.zero;
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
        
        // Handle shape changing - Comenzando por Cube con la tecla 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeShape(CharacterShape.Cube);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeShape(CharacterShape.Tetris1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeShape(CharacterShape.Tetris2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeShape(CharacterShape.Tetris3);
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
        cubeShape.SetActive(false);
        tetrisFigure1.SetActive(false);
        tetrisFigure2.SetActive(false);
        tetrisFigure3.SetActive(false);
        
        // Enable the current shape
        switch (currentShape)
        {
            case CharacterShape.Cube:
                cubeShape.SetActive(true);
                break;
            case CharacterShape.Tetris1:
                tetrisFigure1.SetActive(true);
                break;
            case CharacterShape.Tetris2:
                tetrisFigure2.SetActive(true);
                break;
            case CharacterShape.Tetris3:
                tetrisFigure3.SetActive(true);
                break;
        }
        
        // Ya no necesitamos UpdateCollider() porque cada forma tiene sus propios colliders
        // Opcional: Resetear velocidad al cambiar de forma
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if we've landed on something
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
        
        // Asegurarse de que el personaje no se hunda en el suelo
        if (collision.gameObject.name == "Ground" && transform.position.y < characterHeight)
        {
            transform.position = new Vector3(transform.position.x, characterHeight, transform.position.z);
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Mantener el estado de grounded mientras estamos en contacto con el suelo
        if (collision.gameObject.CompareTag("Untagged") || collision.gameObject.name == "Ground")
        {
            isGrounded = true;
            
            // Mantener la altura correcta
            if (transform.position.y < characterHeight)
            {
                transform.position = new Vector3(transform.position.x, characterHeight, transform.position.z);
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        // Solo perder el estado grounded si abandonamos el suelo
        if (collision.gameObject.CompareTag("Untagged") || collision.gameObject.name == "Ground")
        {
            isGrounded = false;
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
        
        // Deshabilitar todos los colliders del personaje y sus hijos
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in allColliders)
        {
            c.enabled = false;
        }
        
        // Notificar al GameManager
        GameManager gameManager = GameObject.FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
        else
        {
            Debug.LogError("GameManager not found in scene!");
        }
    }
    
    // Método para reiniciar el nivel
    void RestartLevel()
    {
        // Cargar la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    void LateUpdate()
    {
        // Forzar que el personaje mantenga la altura mínima
        if (transform.position.y < characterHeight && isGrounded)
        {
            transform.position = new Vector3(transform.position.x, characterHeight, transform.position.z);
        }
    }
}