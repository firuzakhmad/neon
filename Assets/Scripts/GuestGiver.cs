using UnityEngine;

public class GuestGiver : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3.0f;
    public KeyCode interactionKey = KeyCode.E;


    [Header("UI Elements")]
    public GameObject interactionGuestGiverUI;

    [Header("References")]
    public Transform player;

    private bool playerInRange = false;
    private Quaternion originalRotation;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        originalRotation = transform.rotation;

        if (interactionGuestGiverUI != null)
        {
            interactionGuestGiverUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= interactionDistance;

        if (playerInRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            interactionGuestGiverUI.SetActive(true);
        }
        else if (!playerInRange)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 3f);

            interactionGuestGiverUI.SetActive(false);
        }

        if (interactionGuestGiverUI != null)
        {
            if (playerInRange)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
                interactionGuestGiverUI.transform.position = screenPos;
            }

            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                // interactionGuestGiverUI.SetActive(true);
            }
        }


    }
}