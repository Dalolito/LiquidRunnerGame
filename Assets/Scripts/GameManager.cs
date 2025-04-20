using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool gameOver = false;
    public bool gamePaused = false;
    
    [Header("UI References")]
    public GameObject gameOverPanel;
    
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
    }
    
    void Update()
    {
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
    
    public void GameOver()
    {
         Debug.Log("GameOver() called. Stack trace: " + System.Environment.StackTrace);
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
        ObstacleManager obstacleManager = GameObject.FindAnyObjectByType<ObstacleManager>();
        if (obstacleManager != null)
        {
            obstacleManager.enabled = false;
        }
        
        LiquidCharacterController playerController = GameObject.FindAnyObjectByType<LiquidCharacterController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Desactivar el tiempo del juego
        Time.timeScale = 0f;
        
        // Desactivar el audio del juego
        AudioListener audioListener = FindObjectOfType<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = false;
        }
        
        Debug.Log("Juego detenido. Presiona R para reiniciar.");
    }
    
    public void RestartGame()
    {
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ResetGame()
    {
        gameOver = false;
        gamePaused = false;
        Time.timeScale = 1f;
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