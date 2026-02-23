using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject deathPanel;  
    public Text deathText;         
    
    [Header("Settings")]
    public KeyCode restartKey = KeyCode.R;  
    private bool isPlayerDead = false;
    
    
    public static DeathUI Instance;
    
    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        
        if (isPlayerDead && Input.GetKeyDown(restartKey))
        {
            RestartGame();
        }
    }
    
    public void ShowDeathScreen()
    {
        isPlayerDead = true;
        
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
        
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainScene");
    }
}