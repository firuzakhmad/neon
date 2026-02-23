using UnityEngine;

public class SecurityPassPickup : MonoBehaviour
{
    public GameObject uiIcon;
    public KeyCode pickupKey = KeyCode.E;

    public float interactionDistance = 1.5f;
    public GameObject interactionPick;

    Transform playerTransform;
    PlayerController player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<PlayerController>();
        }

        if (interactionPick != null)
            interactionPick.SetActive(false);

        if (uiIcon != null)
            uiIcon.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool playerInRange = distanceToPlayer <= interactionDistance;

        
        if (interactionPick != null)
        {
            interactionPick.SetActive(playerInRange);

            if (playerInRange)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(
                    transform.position + Vector3.up * 2f
                );

                interactionPick.transform.position = screenPos;
            }
        }

        
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (player != null)
        {
            player.SetSecurityPass(true);
        }

        if (interactionPick != null)
            interactionPick.SetActive(false);

        if (uiIcon != null)
            uiIcon.SetActive(true);

        gameObject.SetActive(false);
    }
}