using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private MovementStateManager movement;
    private CharacterController controller;
    private bool isDead = false;
    
    void Awake()
    {
        movement = GetComponent<MovementStateManager>();
        controller = GetComponent<CharacterController>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if guard touched the player
        if (other.CompareTag("Guard") && !isDead)
        {
            Die();
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Disable player movement
        if (movement != null)
            movement.enabled = false;
            
        if (controller != null)
            controller.enabled = false;
        
        // Show death UI
        if (DeathUI.Instance != null)
        {
            DeathUI.Instance.ShowDeathScreen();
        }
        
        // Optional: Disable other player scripts
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;
            
        AimStateManager aim = GetComponent<AimStateManager>();
        if (aim != null)
            aim.enabled = false;
    }
}