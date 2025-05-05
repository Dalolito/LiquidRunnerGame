using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Movement Settings")]
    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.1f; // Incremento de velocidad por segundo
    public float maxSpeed = 20f; // Velocidad máxima

    [Header("Unified Obstacle Generation")]
    public GameObject[] obstaclePrefabs; // Array de prefabs de obstáculos (muros de lava)
    public GameObject pendulumPrefab;    // Prefab del péndulo
    public float obstacleSpawnRate = 0.14f; // Tasa de aparición de obstáculos (0-1)
    public float pendulumSpawnChance = 0.2f; // Probabilidad de que el obstáculo sea un péndulo
    public float obstacleDistance = 30f; // Distancia a la que aparecen los obstáculos
    public float destroyDistance = -35f; // Distancia a la que se destruyen los obstáculos
    public float minimumObstacleSpace = 12f; // Espacio mínimo entre obstáculos

    [Header("Pendulum Settings")]
    public float pendulumMinHeight = 10f;
    public float pendulumMaxHeight = 10f;

    [Header("Floor Settings")]
    public Transform floorTransform;

    // Variables de control internas
    private float obstacleTimer = 0f;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private float difficulty = 0f; // Contador de dificultad
    private Vector2 textureOffset = Vector2.zero;
    private float distanceSinceLastObstacle = 0f;
    private GameObject lastSpawnedObstacle = null;

    void Start()
    {
        // Si no se asignó un piso, intentar encontrarlo
        if (floorTransform == null)
        {
            GameObject floor = GameObject.Find("Ground");
            if (floor != null)
            {
                floorTransform = floor.transform;
            }
        }
        
        // Inicializar distancia
        distanceSinceLastObstacle = minimumObstacleSpace;
    }

    void Update()
    {
        // Incrementar la dificultad con el tiempo
        difficulty += Time.deltaTime;
        
        // Aumentar la velocidad con el tiempo
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // Calcular el intervalo de aparición basado en la tasa de aparición y la dificultad
        float currentSpawnInterval = Mathf.Max(1f / (obstacleSpawnRate + (difficulty * 0.01f)), 0.5f);
        
        // Calcular espacio mínimo actual (puede disminuir con el tiempo)
        float currentMinSpace = Mathf.Max(minimumObstacleSpace - (difficulty * 0.05f), 5f);
        
        // Actualizar distancia desde el último obstáculo
        if (lastSpawnedObstacle != null)
        {
            distanceSinceLastObstacle = obstacleDistance - lastSpawnedObstacle.transform.position.z;
        }
        else
        {
            distanceSinceLastObstacle += scrollSpeed * Time.deltaTime;
        }
        
        // Generar obstáculos con una combinación de tiempo y espacio
        obstacleTimer += Time.deltaTime;
        
        // Solo generar un nuevo obstáculo si:
        // 1. Ha pasado suficiente tiempo desde el último
        // 2. Hay suficiente espacio desde el último obstáculo
        if (obstacleTimer >= currentSpawnInterval && distanceSinceLastObstacle >= currentMinSpace)
        {
            SpawnObstacle();
            obstacleTimer = 0f;
            distanceSinceLastObstacle = 0f;
        }

        // Mover obstáculos
        MoveObstacles();

        // Animar el piso
        AnimateFloor();
        
        // Depuración
        if (lastSpawnedObstacle != null)
        {
            Debug.DrawLine(
                new Vector3(-5, 1, lastSpawnedObstacle.transform.position.z + currentMinSpace),
                new Vector3(5, 1, lastSpawnedObstacle.transform.position.z + currentMinSpace),
                Color.green
            );
        }
    }

    void SpawnObstacle()
    {
        // Decidir si generar un péndulo o un muro de lava
        bool spawnPendulum = Random.value < pendulumSpawnChance && pendulumPrefab != null;
        
        GameObject newObstacle = null;
        
        if (spawnPendulum)
        {
            // Calcular posición para el péndulo
            float zPosition = obstacleDistance;
            float yPosition = Random.Range(pendulumMinHeight, pendulumMaxHeight);
            Vector3 spawnPosition = new Vector3(0, yPosition, zPosition);
            
            // Instanciar el péndulo
            newObstacle = Instantiate(pendulumPrefab, spawnPosition, Quaternion.identity);
            
            // Configurar fase aleatoria para el péndulo
            PendulumMovement pendulumScript = newObstacle.GetComponent<PendulumMovement>();
            if (pendulumScript != null)
            {
                pendulumScript.phaseOffset = Random.Range(0f, 360f);
            }
            
            // Configurar tags
            newObstacle.tag = "Obstacle";
            foreach (Transform child in newObstacle.transform)
            {
                child.gameObject.tag = "Obstacle";
            }
            
            Debug.Log("Spawned pendulum at " + zPosition);
        }
        else if (obstaclePrefabs.Length > 0)
        {
            // Seleccionar un prefab aleatorio de muro de lava
            int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
            
            // Crear el obstáculo
            Vector3 spawnPosition = new Vector3(3.2f, 1.4f, obstacleDistance);
            newObstacle = Instantiate(obstaclePrefabs[prefabIndex], spawnPosition, Quaternion.identity);
            
            // Configurar tags y colliders
            newObstacle.tag = "Obstacle";
            foreach (Transform child in newObstacle.transform)
            {
                child.gameObject.tag = "Obstacle";
            }
            
            foreach (Collider collider in newObstacle.GetComponentsInChildren<Collider>(true))
            {
                collider.isTrigger = true;
            }
            
            Debug.Log("Spawned lava wall at " + obstacleDistance);
        }
        
        // Registrar y añadir a la lista
        if (newObstacle != null)
        {
            lastSpawnedObstacle = newObstacle;
            activeObstacles.Add(newObstacle);
        }
    }

    void MoveObstacles()
    {
        List<GameObject> obstaclesToRemove = new List<GameObject>();

        // Mover cada obstáculo
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
                
                // Si este es el último obstáculo que generamos, actualizar referencia
                if (obstacle == lastSpawnedObstacle)
                {
                    lastSpawnedObstacle = null;
                    distanceSinceLastObstacle = minimumObstacleSpace; // Permitir generación inmediata
                }
                
                Destroy(obstacle);
            }
        }

        // Eliminar obstáculos destruidos de la lista
        foreach (GameObject obstacle in obstaclesToRemove)
        {
            activeObstacles.Remove(obstacle);
        }
    }

    void AnimateFloor()
    {
        if (floorTransform != null)
        {
            // Obtener el renderer del piso
            Renderer floorRenderer = floorTransform.GetComponent<Renderer>();
            
            if (floorRenderer != null)
            {
                // Actualizar el offset de la textura para dar la impresión de movimiento
                textureOffset.y += scrollSpeed * Time.deltaTime * 0.1f;
                
                // Para URP/HDRP, utiliza las propiedades adecuadas
                if (floorRenderer.material.HasProperty("_BaseMap"))
                {
                    floorRenderer.material.SetTextureOffset("_BaseMap", textureOffset);
                }
                else if (floorRenderer.material.HasProperty("_MainTex"))
                {
                    // Sólo aplicar el offset a la textura principal si existe
                    floorRenderer.material.SetTextureOffset("_MainTex", textureOffset);
                }
            }
        }
    }

    // Método para visualizar parámetros en el editor
    void OnDrawGizmos()
    {
        // Distancia de generación
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(-5, 1, obstacleDistance), 
            new Vector3(5, 1, obstacleDistance)
        );
        
        // Espacio mínimo
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-5, 1, obstacleDistance - minimumObstacleSpace), 
            new Vector3(5, 1, obstacleDistance - minimumObstacleSpace)
        );
    }
}