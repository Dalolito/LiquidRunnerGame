using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Movement Settings")]
    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.1f;
    public float maxSpeed = 20f;

    [Header("Obstacle Generation")]
    public GameObject[] obstaclePrefabs; // Todos los obstáculos de lava
    public GameObject pendulumPrefab;    // Prefab específico para péndulo
    public float pendulumChance = 0.25f; // Probabilidad de generar péndulo vs muro de lava
    public float spawnInterval = 3f; 
    public float minSpawnInterval = 1f;
    public float obstacleDistance = 30f; // Distancia Z fija para todos los obstáculos
    public float destroyDistance = -35f;
    public float minimumObstacleSpace = 12f; // Espacio mínimo entre obstáculos normales
    public float pendulumExtraSpace = 8f;    // Espacio adicional después de un péndulo

    [Header("Floor Settings")]
    public Transform floorTransform;

    // Control variables
    private float obstacleTimer = 0f;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private float difficulty = 0f;
    private Vector2 textureOffset = Vector2.zero;
    private bool lastWasPendulum = false;     // Para rastrear si el último obstáculo fue un péndulo

    [Header("Offset Settings")]
    public float pendulumZOffset = -20f;  // Ajuste de posición Z para péndulos (negativo = más lejos)

    void Start()
    {
        if (floorTransform == null)
        {
            GameObject floor = GameObject.Find("Ground");
            if (floor != null)
            {
                floorTransform = floor.transform;
            }
        }
        
        // Debug de los prefabs
        Debug.Log("Prefabs configurados: " + (obstaclePrefabs != null ? obstaclePrefabs.Length : 0) + " muros, " + 
                 (pendulumPrefab != null ? "1" : "0") + " péndulo");
    }

    void Update()
    {
        // Incrementar la dificultad y velocidad
        difficulty += Time.deltaTime;
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // Calcular intervalo basado en dificultad
        float currentSpawnInterval = Mathf.Max(spawnInterval - (difficulty * 0.05f), minSpawnInterval);
        
        // Si el último fue un péndulo, añadir tiempo de espera adicional
        if (lastWasPendulum)
        {
            currentSpawnInterval += pendulumExtraSpace / scrollSpeed;
        }
        
        // Incrementar timer
        obstacleTimer += Time.deltaTime;
        
        // Verificar si es tiempo de generar un nuevo obstáculo
        if (obstacleTimer >= currentSpawnInterval)
        {
            // Decidir entre muro de lava o péndulo
            bool spawnPendulum = Random.value < pendulumChance && pendulumPrefab != null;
            
            // Si el último fue un péndulo, evitar generar otro péndulo
            if (lastWasPendulum)
            {
                spawnPendulum = false;
            }
            
            if (spawnPendulum)
            {
                SpawnPendulum();
                lastWasPendulum = true;
            }
            else
            {
                SpawnLavaWall();
                lastWasPendulum = false;
            }
            
            // Resetear el timer
            obstacleTimer = 0f;
        }

        // Mover obstáculos existentes
        MoveObstacles();
        
        // Animar el suelo
        AnimateFloor();
    }

    void SpawnLavaWall()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("No hay prefabs de muro de lava configurados");
            return;
        }
            
        // Seleccionar un prefab aleatorio
        int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
        
        // IMPORTANTE: Crear un nuevo GameObject vacío que servirá como contenedor
        GameObject container = new GameObject("LavaWallContainer");
        container.transform.position = new Vector3(0, 0, obstacleDistance); // Posición Z exacta
        
        // Instanciar el muro como hijo del contenedor
        GameObject wall = Instantiate(obstaclePrefabs[prefabIndex], container.transform);
        
        // Ajustar la posición local del muro para que esté a la derecha
        wall.transform.localPosition = new Vector3(3.2f, 1.4f, 0);
        
        // Asegurarse de que el obstáculo y sus hijos tienen el tag "Obstacle"
        container.tag = "Obstacle";
        wall.tag = "Obstacle";
        foreach (Transform child in wall.transform)
        {
            child.gameObject.tag = "Obstacle";
        }
        
        // Asegurarse de que los colliders estén configurados como triggers
        foreach (Collider collider in wall.GetComponentsInChildren<Collider>(true))
        {
            collider.isTrigger = true;
        }
        
        // Añadir a la lista de obstáculos activos
        activeObstacles.Add(container);
        
        Debug.Log("Muro de lava generado con posición Z = " + container.transform.position.z);
    }
    
    void SpawnPendulum()
    {
        if (pendulumPrefab == null)
        {
            Debug.LogError("No hay prefab de péndulo configurado");
            return;
        }
        
        // IMPORTANTE: Crear un nuevo GameObject vacío que servirá como contenedor
        GameObject container = new GameObject("PendulumContainer");
       // Aplicar la posición z con el offset configurable
        container.transform.position = new Vector3(0, 0, obstacleDistance + pendulumZOffset);
        
        // Instanciar el péndulo como hijo del contenedor
        GameObject pendulum = Instantiate(pendulumPrefab, container.transform);
        
        // Ajustar la posición local del péndulo para que esté a la altura correcta
        pendulum.transform.localPosition = new Vector3(0, 10f, 0);
        
        // Configurar el componente PendulumMovement si existe
        PendulumMovement movement = pendulum.GetComponentInChildren<PendulumMovement>();
        if (movement != null)
        {
            movement.phaseOffset = Random.Range(0f, 360f);
        }
        
        // Asegurarse de que el péndulo y sus hijos tienen el tag "Obstacle"
        container.tag = "Obstacle";
        pendulum.tag = "Obstacle";
        foreach (Transform child in pendulum.transform)
        {
            child.gameObject.tag = "Obstacle";
        }
        
        // Añadir a la lista de obstáculos activos
        activeObstacles.Add(container);
        
        Debug.Log("Péndulo generado con posición Z = " + container.transform.position.z);
    }

    void MoveObstacles()
    {
        List<GameObject> obstaclesToRemove = new List<GameObject>();

        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle == null)
                continue;
                
            // Mover el obstáculo
            obstacle.transform.Translate(0, 0, -scrollSpeed * Time.deltaTime);

            // Verificar si debe ser destruido
            if (obstacle.transform.position.z < destroyDistance)
            {
                obstaclesToRemove.Add(obstacle);
                Destroy(obstacle);
            }
        }

        // Eliminar referencias a obstáculos destruidos
        foreach (GameObject obstacle in obstaclesToRemove)
        {
            activeObstacles.Remove(obstacle);
        }
    }

    void AnimateFloor()
    {
        if (floorTransform != null)
        {
            Renderer floorRenderer = floorTransform.GetComponent<Renderer>();
            
            if (floorRenderer != null)
            {
                textureOffset.y += scrollSpeed * Time.deltaTime * 0.1f;
                
                if (floorRenderer.material.HasProperty("_BaseMap"))
                {
                    floorRenderer.material.SetTextureOffset("_BaseMap", textureOffset);
                }
                else if (floorRenderer.material.HasProperty("_MainTex"))
                {
                    floorRenderer.material.SetTextureOffset("_MainTex", textureOffset);
                }
            }
        }
    }

    // Para visualizar la posición de generación
    void OnDrawGizmos()
    {
        // Dibujar una línea que muestra la posición Z de generación
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-10, 0, obstacleDistance), new Vector3(10, 0, obstacleDistance));
    }
}