using UnityEngine;
using UnityEngine.UI;

public class Shopper : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("UI Elements")]
    public GameObject interactionPromptUI;
    public GameObject shopPanel;
    public GameObject SecurityTool;
    
    [Header("Item Settings")]
    public string itemName = "Crowbar";
    public int itemPrice = 50;
    
    [Header("References")]
    public Transform player;
    
    private bool playerInRange = false;
    private bool shopOpen = false;
    private bool itemPurchased = false;
    private Quaternion originalRotation;

    void Start()
    {
    
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
    
        originalRotation = transform.rotation;
        
    
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
            
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;
        
    
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= interactionDistance;
        
    
        if (playerInRange && !shopOpen)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else if (!playerInRange)
        {
        
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 3f);
        }
        
    
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(playerInRange && !shopOpen && !itemPurchased);
            
            if (playerInRange)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
                interactionPromptUI.transform.position = screenPos;
            }
        }
        
    
        if (playerInRange && Input.GetKeyDown(interactKey) && !shopOpen && !itemPurchased)
        {
            SecurityTool.SetActive(true);
        }
        
    }

    void OpenShop()
    {
        shopOpen = true;
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            
        
            Text panelText = shopPanel.GetComponentInChildren<Text>();
            if (panelText != null)
            {
                panelText.text = $"Buy {itemName} for ${itemPrice}?\n\nPress Y to Confirm\nPress N to Cancel";
            }
        }
        
    
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
            
    
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    
        Time.timeScale = 0f;
    }

    void CloseShop()
    {
        shopOpen = false;
        
        if (shopPanel != null)
            shopPanel.SetActive(false);
            
    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    
        Time.timeScale = 1f;
    }

    public void PurchaseItem()
    {
        if (itemPurchased) return;
        
        itemPurchased = true;
        
    
        ShowCrowbarInInventory();
        
    
        CloseShop();
        
        Debug.Log($"{itemName} purchased!");
    }

    void ShowCrowbarInInventory()
    {
    
        GameObject inventoryPanel = GameObject.Find("BottomLeftInventory");
        if (inventoryPanel == null)
        {
        
            inventoryPanel = new GameObject("BottomLeftInventory");
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                inventoryPanel.transform.SetParent(canvas.transform);
            }
            
            RectTransform rect = inventoryPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(10, 10);
            rect.sizeDelta = new Vector2(100, 100);
        }
        
    
        GameObject crowbarIcon = new GameObject("CrowbarIcon");
        crowbarIcon.transform.SetParent(inventoryPanel.transform);
        
        Image image = crowbarIcon.AddComponent<Image>();
        image.color = Color.red;
        
        RectTransform iconRect = crowbarIcon.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(80, 80);
        iconRect.anchoredPosition = Vector2.zero;
    }

    void OnTriggerEnter(Collider other)
    {
    
        if (other.CompareTag("Player"))
        {
        
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}