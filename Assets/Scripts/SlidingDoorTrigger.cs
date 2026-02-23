using UnityEngine;

public class SlidingDoorTrigger : MonoBehaviour
{
    public Animator doorAnimation;
    public string PlayerTag;
    public string OpenCloseAnimationBoolName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == PlayerTag)
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController.HasSecurityPassTool)
            {
                doorAnimation.SetBool(OpenCloseAnimationBoolName, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == PlayerTag)
        {
            doorAnimation.SetBool(OpenCloseAnimationBoolName, false);
        }
    }
}
