using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Movement Settings")]
    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.1f; // Incremento de velocidad por segundo
    public float maxSpeed = 20f; // Velocidad máxima

    [Header("Obstacle Generation")]
    public GameObject[] obstaclePrefabs; // Array de prefabs de obstáculos (muros de lava)
    public float spawnInterval = 3f; // Tiempo entre generación de obstáculos
    public float minSpawnInterval = 0.5f; // Tiempo mínimo entre obstáculos
    public float obstacleDistance = 30f; // Distancia a la que aparecen los obstáculos
    public float destroyDistance = -30f; // Distancia a la que se destruyen los obstáculos

    [Header("Floor Settings")]
    public Transform floorTransform; // Referencia al piso

    private float timer = 0f;
    private float currentSpawnInterval;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private float gameTime = 0f;
    private Vector2 textureOffset = Vector2.zero;

    void Start()
    {
        // Inicializar valores
        currentSpawnInterval = spawnInterval;
        
        // Si no se asignó un piso, intentar encontrarlo
        if (floorTransform == null)
        {
            GameObject floor = GameObject.Find("Ground");
            if (floor != null)
            {
                floorTransform = floor.transform;
            }
        }
    }

    void Update()
    {
        // Incrementar el tiempo de juego
        gameTime += Time.deltaTime;

        // Aumentar la velocidad con el tiempo
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // Generar obstáculos
        timer += Time.deltaTime;
        if (timer >= currentSpawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
            // Reducir el intervalo de generación con el tiempo
            currentSpawnInterval = Mathf.Max(currentSpawnInterval - 0.05f, minSpawnInterval);
        }

        // Mover obstáculos
        MoveObstacles();

        // Animar el piso (efecto de movimiento de textura)
        AnimateFloor();
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0)
            return;

        // Seleccionar un prefab aleatorio
        int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
        
        // Crear el obstáculo
        Vector3 spawnPosition = new Vector3(3.2f, 1.4f, obstacleDistance);
        GameObject obstacle = Instantiate(obstaclePrefabs[prefabIndex], spawnPosition, Quaternion.identity);
        
        // Asegurarse de que el obstáculo y sus hijos tienen el tag "Obstacle"
        obstacle.tag = "Obstacle";
        foreach (Transform child in obstacle.transform)
        {
            child.gameObject.tag = "Obstacle";
        }
        
        // Asegurarse de que los colliders estén configurados como triggers
        foreach (Collider collider in obstacle.GetComponentsInChildren<Collider>(true))
        {
            collider.isTrigger = true;
        }
        
        // Añadir a la lista de obstáculos activos
        activeObstacles.Add(obstacle);
    }
    void MoveObstacles()
    {
        List<GameObject> obstaclesToRemove = new List<GameObject>();

        // Mover cada obstáculo
        foreach (GameObject obstacle in activeObstacles)
        {
            // Mover el obstáculo
            obstacle.transform.Translate(0, 0, -scrollSpeed * Time.deltaTime);

            // Verificar si debe ser destruido
            if (obstacle.transform.position.z < destroyDistance)
            {
                obstaclesToRemove.Add(obstacle);
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
                
                // Aplicar el offset a la textura principal
                floorRenderer.material.SetTextureOffset("_MainTex", textureOffset);
                
                // Si estás usando URP/HDRP, también puedes necesitar actualizar estas propiedades
                if (floorRenderer.material.HasProperty("_BaseMap"))
                {
                    floorRenderer.material.SetTextureOffset("_BaseMap", textureOffset);
                }
            }
        }
    }

    // Método para crear un muro de lava con un patrón específico (puedes implementar diferentes patrones)
    GameObject CreateLavaWall(Vector3 position)
    {
        // Aquí implementarías la lógica para crear un muro de lava con diferentes formas de agujeros
        // Por ahora, simplemente devuelve un prefab existente
        if (obstaclePrefabs.Length > 0)
        {
            return Instantiate(obstaclePrefabs[0], position, Quaternion.identity);
        }
        return null;
    }
}