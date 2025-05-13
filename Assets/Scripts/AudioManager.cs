using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    public bool loopMusic = true;
    public bool playOnAwake = true;
    
    [Header("Advanced Settings")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 2f;
    public bool dontDestroyOnLoad = true;
    
    private AudioSource musicSource;
    private Coroutine fadeCoroutine;
    private bool isMusicStopped = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Crear fuente de audio para la música
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.volume = 0;  // Empezamos en 0 para hacer fade in
        musicSource.loop = loopMusic;
        musicSource.playOnAwake = false;
        
        if (playOnAwake && backgroundMusic != null)
            PlayMusic();
    }
    
    void Start()
    {
        // Suscribirse al evento de nivel cargado para reiniciar la música
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Evento que se ejecuta cuando se carga una escena
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Si la música estaba detenida y se cargó una nueva escena, reiniciarla
        if (isMusicStopped)
        {
            PlayMusic();
            isMusicStopped = false;
        }
    }
    
    public void PlayMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        
        // Detener cualquier fade anterior
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        // Si la música ya estaba reproduciéndose, detenerla
        if (musicSource.isPlaying)
            musicSource.Stop();
            
        // Comenzar a reproducir y hacer fade in
        musicSource.Play();
        fadeCoroutine = StartCoroutine(FadeMusicVolume(0f, musicVolume, fadeInDuration));
    }
    
    public void StopMusic()
    {
        if (musicSource == null) return;
        
        // Marcar la música como detenida
        isMusicStopped = true;
        
        // Detener cualquier fade anterior
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        // Hacer fade out y luego detener
        fadeCoroutine = StartCoroutine(FadeMusicAndStop(fadeOutDuration));
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.UnPause();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
    
    // Corrutina para hacer un fade de volumen suave
    private IEnumerator FadeMusicVolume(float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        musicSource.volume = targetVolume;
        fadeCoroutine = null;
    }
    
    // Corrutina para hacer fade out y luego detener
    private IEnumerator FadeMusicAndStop(float duration)
    {
        float startVolume = musicSource.volume;
        
        yield return FadeMusicVolume(startVolume, 0f, duration);
        
        musicSource.Stop();
    }
    
    // Este método se puede llamar desde GameManager cuando el juego termina
    public void OnGameOver()
    {
        // Opcional: Puedes hacer un fade out de la música o cambiarla por una de game over
        StopMusic();
    }
    
    // Para integrar con GameManager, vincular la pausa
    public void OnGamePaused(bool isPaused)
    {
        if (isPaused)
            PauseMusic();
        else
            ResumeMusic();
    }
}