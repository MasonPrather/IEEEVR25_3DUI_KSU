using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Threading.Tasks;

public class MultiplayerLobbyUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text statusText; // Text to display lobby status
    [SerializeField] private Button startButton; // Button to start the game

    [Header("Lobby Panel Prefab")]
    public GameObject lobbyPanelPrefab; // Drag your LobbyPanel prefab here in the Inspector.
    private GameObject currentLobbyPanel;

    private Lobby connectedLobby;
    private bool isHost;

    private async void Start()
    {
        // Initialize UI
        statusText.text = "Waiting for player 2...";
        startButton.gameObject.SetActive(false);

        // Start listening for lobby updates
        await PollLobbyForUpdates();
    }

    public async Task SetupLobby(bool host, Lobby lobby)
    {
        connectedLobby = lobby;
        isHost = host;

        if (connectedLobby != null)
        {
            Debug.Log("Lobby setup successful.");
        }
        else
        {
            Debug.LogError("Failed to setup lobby.");
        }
    }

    public void ShowLobbyPanel()
    {
        // Instantiate the LobbyPanel prefab if it doesn't already exist.
        if (currentLobbyPanel == null)
        {
            currentLobbyPanel = Instantiate(lobbyPanelPrefab, transform);
        }

        // Ensure it is active and positioned correctly.
        currentLobbyPanel.SetActive(true);
    }

    public void HideLobbyPanel()
    {
        // Deactivate the lobby panel but don't destroy it (optional).
        if (currentLobbyPanel != null)
        {
            currentLobbyPanel.SetActive(false);
        }
    }

    private async Task PollLobbyForUpdates()
    {
        while (connectedLobby != null)
        {
            try
            {
                // Refresh the lobby state
                connectedLobby = await LobbyService.Instance.GetLobbyAsync(connectedLobby.Id);

                if (connectedLobby.Players.Count >= 2)
                {
                    statusText.text = "Ready to go!";
                    startButton.gameObject.SetActive(true);

                    if (isHost)
                    {
                        startButton.onClick.AddListener(OnStartButtonPressed);
                    }

                    break;
                }
                else
                {
                    statusText.text = "Waiting for player 2...";
                    startButton.gameObject.SetActive(false);
                }
            }
            catch
            {
                Debug.LogError("Error updating lobby state.");
                break;
            }

            // Wait for a few seconds before checking again
            await Task.Delay(3000);
        }
    }

    private void OnStartButtonPressed()
    {
        Debug.Log("starting level");

        // Add your level-loading logic here (e.g., SceneManager.LoadScene())
    }

    private void OnDestroy()
    {
        // Clean up the listener
        startButton.onClick.RemoveAllListeners();
    }
}