using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;

/// <summary>
/// 
/// Project: IEEE VR 2025 - 3D UI Contest
/// Author: Mason Prather
/// Title: Multiplayer Manager
/// 
/// Description: Handles all multiplayer management, including networking, lobby, and relay.
/// 
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    // Constants for Lobby Data
    public const string k_JoinCodeKeyIdentifier = "j";

    [Header("Unity Services Settings")]
    [Tooltip("Enable verbose logging for debugging Unity Services.")]
    public bool enableLogging = false;

    [Header("Network Settings")]
    [Tooltip("Transport for network connections.")]
    public UnityTransport transport;

    [Tooltip("Maximum players allowed in the lobby.")]
    public int maxPlayers = 4;

    private Lobby currentLobby;
    private Coroutine heartbeatRoutine;

    void Awake()
    {
        // Ensure UnityTransport is set
        if (transport == null)
        {
            transport = FindObjectOfType<UnityTransport>();
        }

        // Initialize Unity Services
        InitializeUnityServices();
    }

    private async void InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Log("Signed in anonymously.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Unity Services Initialization failed: {e.Message}");
        }
    }

    /// <summary>
    /// Quick join or create a lobby.
    /// </summary>
    public async Task JoinOrCreateLobby()
    {
        try
        {
            Log("Attempting to quick join a lobby...");
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Log($"Joined lobby: {currentLobby.Id}");
            await SetupRelay(currentLobby);
        }
        catch
        {
            Log("No available lobbies. Creating a new one...");
            currentLobby = await CreateLobby();
        }
    }

    /// <summary>
    /// Create a new lobby.
    /// </summary>
    private async Task<Lobby> CreateLobby()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var createLobbyOptions = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { k_JoinCodeKeyIdentifier, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                },
                IsPrivate = false
            };

            var newLobby = await LobbyService.Instance.CreateLobbyAsync("New Lobby", maxPlayers, createLobbyOptions);
            Log($"Created lobby: {newLobby.Id} with join code: {joinCode}");

            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            StartHeartbeat(newLobby.Id);
            return newLobby;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Sets up relay for joining a lobby.
    /// </summary>
    private async Task SetupRelay(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[k_JoinCodeKeyIdentifier].Value);
            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            Log("Relay setup completed.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay setup failed: {e.Message}");
        }
    }

    /// <summary>
    /// Starts the heartbeat to keep the lobby alive.
    /// </summary>
    private void StartHeartbeat(string lobbyId)
    {
        if (heartbeatRoutine != null)
        {
            StopCoroutine(heartbeatRoutine);
        }
        heartbeatRoutine = StartCoroutine(HeartbeatCoroutine(lobbyId));
    }

    private IEnumerator HeartbeatCoroutine(string lobbyId)
    {
        var wait = new WaitForSecondsRealtime(15);
        while (true)
        {
            yield return wait;
            try
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                Log("Lobby heartbeat sent.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Heartbeat failed: {e.Message}");
                yield break;
            }
        }
    }

    /// <summary>
    /// Disconnects from the current lobby.
    /// </summary>
    public async Task LeaveLobby()
    {
        if (heartbeatRoutine != null)
        {
            StopCoroutine(heartbeatRoutine);
        }

        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                Log("Left the lobby.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to leave the lobby: {e.Message}");
            }
            finally
            {
                currentLobby = null;
            }
        }
    }

    /// <summary>
    /// Logs messages if logging is enabled.
    /// </summary>
    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[MultiplayerManager] {message}");
        }
    }
}
