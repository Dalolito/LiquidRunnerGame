using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool gameOver = false;
    public bool gamePaused = false;
    
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    
    [Header("Score Settings")]
    public float score = 0;
    public float scoreMultiplier = 1f;
    public float scoreMultiplierIncreaseRate = 0.1f; // Incremento del multiplicador por segundo
    public float maxScoreMultiplier = 5f; // Máximo multiplicador de puntaje
    
    private void Awake()
    {
        // Singleton sin persistencia entre escenas
        if (Instance == null)
        {
            Instance = this;
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
    }
    
    void Update()
    {
        // Si el juego está activo, aumentar el puntaje y el multiplicador
        if (!gameOver && !gamePaused)
        {
            // Aumentar el multiplicador con el tiempo
            scoreMultiplier = Mathf.Min(scoreMultiplier + scoreMultiplierIncreaseRate * Time.deltaTime, maxScoreMultiplier);
            
            // Aumentar el puntaje basado en el multiplicador
            score += Time.deltaTime * scoreMultiplier * 10; // Multiplicamos por 10 para que suba más rápido
            
            // Actualizar la UI del puntaje
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
        
        // Actualizar el puntaje final en el panel de Game Over
        if (finalScoreText != null)
        {
            finalScoreText.text = "Puntaje Final: " + Mathf.Floor(score).ToString();
        }
        
        // Mostrar panel de game over si existe
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Debug.Log("GAME OVER - Puntaje final: " + Mathf.Floor(score));
        
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
        scoreMultiplier = 1f;
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
        
        // Actualizar la UI del puntaje
        UpdateScoreUI();
        
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