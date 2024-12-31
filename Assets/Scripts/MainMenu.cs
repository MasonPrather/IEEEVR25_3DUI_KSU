using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button enterGameButton;

    private void Start()
    {
        enterGameButton.onClick.AddListener(JoinOrCreateLobby);
    }

    public async void JoinOrCreateLobby()
    { 
        Debug.Log("Joining or creating a lobby...");
        var lobbyManager = FindObjectOfType<MultiplayerManager>();
        if (lobbyManager != null)
        {
            await lobbyManager.JoinOrCreateLobby();
        }
        else
        {
            Debug.LogError("LobbyManager not found in the scene!");
        }
    }
}