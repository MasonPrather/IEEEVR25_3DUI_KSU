using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RelayManager relayManager;
    public Button hostButton;
    public Button joinButton;

    void Start()
    {
        hostButton.onClick.AddListener(async () => await relayManager.HostGame());
        joinButton.onClick.AddListener(async () => await relayManager.JoinGame());
    }
}