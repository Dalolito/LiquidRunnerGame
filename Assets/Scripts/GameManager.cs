using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Para interactuar con elementos UI
using TMPro; // Para TextMeshPro

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool gameOver = false;
    public bool gamePaused = false;
    
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText; // Para mostrar el puntaje (opcional)
    public Button restartButton; // Referencia al botón de reiniciar
    
    [Header("Game Settings")]
    public float score = 0;
    public float scoreMultiplier = 1;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Inicializar el juego
        ResetGame();
        
        // Ocultar panel de game over si existe
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Configurar el listener del botón de reinicio si existe
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }
    
    void Update()
    {
        // Si el juego está activo, aumentar el puntaje
        if (!gameOver && !gamePaused)
        {
            score += Time.deltaTime * scoreMultiplier;
            UpdateScoreUI();
        }
        
        // Si el juego ha terminado, revisar si el jugador quiere reiniciar
        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        
        // Pausar/Reanudar el juego con la tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    void UpdateScoreUI()
    {
        // Actualizar el texto del puntaje, si existe
        if (scoreText != null)
        {
            scoreText.text = "Puntaje: " + Mathf.Floor(score).ToString();
        }
    }
    
    public void GameOver()
    {
        if (gameOver)
            return;
            
        gameOver = true;
        
        // Mostrar panel de game over si existe
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Debug.Log("GAME OVER - Presiona R para reiniciar");
        
        // Detener todos los sistemas del juego
        ObstacleManager obstacleManager = FindObjectOfType<ObstacleManager>();
        if (obstacleManager != null)
        {
            obstacleManager.enabled = false;
        }
        
        LiquidCharacterController playerController = FindObjectOfType<LiquidCharacterController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // No detenemos completamente el tiempo para permitir animaciones de UI
        Time.timeScale = 0.1f;
    }
    
    public void RestartGame()
    {
        // Desactivar UI antes de recargar
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Restaurar tiempo
        Time.timeScale = 1f;
        
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        // Importante: actualizar las referencias después de cargar la escena
        SceneManager.sceneLoaded += OnSceneReloaded;
    }

    private void OnSceneReloaded(Scene scene, LoadSceneMode mode)
    {
        // Actualizar referencias a objetos UI
        gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Buscar el botón de reinicio
        GameObject restartButtonObj = GameObject.Find("RestartButton");
        if (restartButtonObj != null)
        {
            restartButton = restartButtonObj.GetComponent<Button>();
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }
        }
        
        // Importante: quitar este listener para evitar múltiples registros
        SceneManager.sceneLoaded -= OnSceneReloaded;
        
        // Resetear variables
        gameOver = false;
        gamePaused = false;
        score = 0;
}
    public void ResetGame()
    {
        // Asegurarse de que el panel de game over esté desactivado
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Resetear todas las variables de estado
        gameOver = false;
        gamePaused = false;
        score = 0;
        Time.timeScale = 1f;
        
        // Reactivar los sistemas del juego si existen
        ObstacleManager obstacleManager = FindObjectOfType<ObstacleManager>();
        if (obstacleManager != null)
        {
            obstacleManager.enabled = true;
        }
        
        LiquidCharacterController playerController = FindObjectOfType<LiquidCharacterController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        Debug.Log("Game Reset - All systems reinitialized");
    }
    
    public void PauseGame()
    {
        if (gameOver)
            return;
            
        gamePaused = true;
        Time.timeScale = 0f;
        
        Debug.Log("Juego Pausado - Presiona ESC para continuar");
    }
    
    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        
        Debug.Log("Juego Reanudado");
    }
}