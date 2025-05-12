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

    [Header("Effects")]
    public GameObject transformEffectPrefab;
    
    void Start()
    {
        // Añadir o configurar Rigidbody si es necesario
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Added Rigidbody to player");
        }
        
        // Configurar el Rigidbody para una mejor detección de colisiones
        rb.mass = 1.0f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.freezeRotation = true; // Evita que el personaje gire
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Mejor detección para objetos rápidos

        currentHealth = maxHealth;
        
        // Asegurarse de que el GameObject tiene el tag Player
        gameObject.tag = "Player";
        foreach (Transform child in transform)
        {
            child.gameObject.tag = "Player";
        }
        
        // Asegurar que todos los elementos tienen los colliders configurados correctamente
        ConfigureColliders();
        
        // Centrar todas las figuras
        CenterAllFigures();
        
        // Elevar el personaje completo
        transform.position = new Vector3(transform.position.x, characterHeight, transform.position.z);
        
        UpdateCharacterShape();
        
        // Debug
        Debug.Log("Personaje inicializado con tag: " + gameObject.tag + " y Rigidbody configurado: " + rb);
    }
    
    // Método para asegurar que todos los colliders están bien configurados
    void ConfigureColliders()
    {
        // Asegurarse de que el personaje principal tiene un collider
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(2, 2, 1); // Tamaño que abarque todas las formas
            boxCollider.center = new Vector3(0, 0, 0);
            boxCollider.isTrigger = false; // No es trigger para detectar colisiones físicas
            Debug.Log("Added BoxCollider to main player object");
        }
        
        // Asegurarse de que cada parte del personaje tiene el tag correcto
        if (cubeShape != null) cubeShape.tag = "Player";
        if (tetrisFigure1 != null) tetrisFigure1.tag = "Player";
        if (tetrisFigure2 != null) tetrisFigure2.tag = "Player";
        if (tetrisFigure3 != null) tetrisFigure3.tag = "Player";
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
        if (currentShape != newShape)
        {
            // Crear efecto de transformación
            if (transformEffectPrefab != null)
            {
                // Instanciar el efecto
                GameObject effect = Instantiate(transformEffectPrefab, transform.position, Quaternion.identity);
                
                // Obtener el tamaño aproximado de la forma actual y la nueva
                Vector3 currentSize = GetShapeSize(currentShape);
                Vector3 newSize = GetShapeSize(newShape);
                
                // Usar el tamaño mayor entre las dos formas para asegurar cobertura completa
                float maxSize = Mathf.Max(
                    Mathf.Max(currentSize.x, currentSize.y, currentSize.z),
                    Mathf.Max(newSize.x, newSize.y, newSize.z)
                );
                
                // Ajustar el sistema de partículas
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    // Ajustar el tamaño de las partículas
                    var main = ps.main;
                    main.startSize = maxSize * 0.4f; // 40% del tamaño máximo
                    
                    // Ajustar el radio de emisión
                    var shape = ps.shape;
                    shape.radius = maxSize * 0.6f; // 60% del tamaño máximo
                    
                    // Aumentar el número de partículas para formas más grandes
                    var emission = ps.emission;
                    ParticleSystem.Burst burst = emission.GetBurst(0);
                    burst.count = 20 + (int)(maxSize * 10); // Más partículas para formas más grandes
                    emission.SetBurst(0, burst);
                }
                
                Destroy(effect, 1.0f);
            }
            
            currentShape = newShape;
            UpdateCharacterShape();
        }
    }

    // Método auxiliar para estimar el tamaño de cada forma
    private Vector3 GetShapeSize(CharacterShape shape)
    {
        switch (shape)
        {
            case CharacterShape.Cube:
                return cubeShape != null ? cubeShape.transform.localScale : Vector3.one;
                
            case CharacterShape.Tetris1:
                return tetrisFigure1 != null ? new Vector3(3f, 4f, 1f) : Vector3.one; // Ajustar según tus formas
                
            case CharacterShape.Tetris2:
                return tetrisFigure2 != null ? new Vector3(3.5f, 3f, 1f) : Vector3.one; // Ajustar según tus formas
                
            case CharacterShape.Tetris3:
                return tetrisFigure3 != null ? new Vector3(3f, 3.5f, 1f) : Vector3.one; // Ajustar según tus formas
                
            default:
                return Vector3.one;
        }
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
        
        // Verificar colisión con obstáculos
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Player collided with obstacle: " + collision.gameObject.name);
            
            LavaWall lavaWall = collision.gameObject.GetComponent<LavaWall>();
            if (lavaWall != null)
            {
                if (lavaWall.isDeadly)
                {
                    Die();
                }
                else
                {
                    TakeDamage(lavaWall.damage);
                }
            }
            else
            {
                // Si no tiene el componente LavaWall, buscar en los hijos
                LavaWall[] childWalls = collision.gameObject.GetComponentsInChildren<LavaWall>();
                if (childWalls.Length > 0)
                {
                    if (childWalls[0].isDeadly)
                    {
                        Die();
                    }
                    else
                    {
                        TakeDamage(childWalls[0].damage);
                    }
                }
                else
                {
                    // Si no se encuentra ningún componente LavaWall, asumir que es mortal
                    Die();
                }
            }
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
    
    // Añadimos detección de triggers para los obstáculos configurados como triggers
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player triggered with: " + other.gameObject.name + " (tag: " + other.tag + ")");
        
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Player triggered with obstacle!");
            
            LavaWall lavaWall = other.GetComponent<LavaWall>();
            if (lavaWall != null)
            {
                if (lavaWall.isDeadly)
                {
                    Debug.Log("Deadly obstacle - calling Die()");
                    Die();
                }
                else
                {
                    Debug.Log("Non-deadly obstacle - applying damage: " + lavaWall.damage);
                    TakeDamage(lavaWall.damage);
                }
            }
            else
            {
                // Si no se encuentra el componente LavaWall, buscar en los padres o asumir que es mortal
                LavaWall parentWall = other.GetComponentInParent<LavaWall>();
                if (parentWall != null)
                {
                    if (parentWall.isDeadly)
                    {
                        Die();
                    }
                    else
                    {
                        TakeDamage(parentWall.damage);
                    }
                }
                else
                {
                    // Si no se encuentra ningún componente LavaWall, asumir que es mortal
                    Debug.Log("No LavaWall component found on obstacle, assuming deadly");
                    Die();
                }
            }
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
            Debug.Log("Calling GameManager.GameOver()");
            gameManager.GameOver();
        }
        else
        {
            Debug.LogError("GameManager not found in scene!");
            // Intentar cargar la escena actual como respaldo
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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
    
    // Método para mostrar información de depuración en pantalla
    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Player Health: " + currentHealth);
            GUI.Label(new Rect(10, 30, 300, 20), "Player Shape: " + currentShape);
            GUI.Label(new Rect(10, 50, 300, 20), "Grounded: " + isGrounded);
        }
    }
}