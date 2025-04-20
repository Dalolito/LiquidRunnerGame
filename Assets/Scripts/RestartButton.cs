using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        // Obtener el componente Button
        button = GetComponent<Button>();
        
        // Añadir listener para el evento onClick
        if (button != null)
        {
            button.onClick.AddListener(RestartGame);
        }
    }

    void RestartGame()
    {
        // Encontrar el GameManager y llamar a su método RestartGame
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            // Si no se encuentra el GameManager, reiniciar directamente
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}