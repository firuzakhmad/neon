using UnityEngine;
using UnityEngine.SceneManagement;

public class GameReset : MonoBehaviour
{
    public void ResetGame()
    {
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
       SceneManager.LoadScene("MainScene");

    }
}