using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
/// <summary>
/// 
/// Project: IEEE VR 2025 - 3D UI Contest
/// Author: Mason Prather
/// Title: Relay Manager
/// 
/// Description: This script is responsible for managing Unity Relay connections for hosting and joining multiplayer sessions. 
///              It initializes Unity Services, creates a static allocation for a multiplayer session, and allows players to 
///              host or join the same room without requiring manual join code input. The script ensures seamless 
///              integration with Unity's Netcode for GameObjects.
/// 
/// </summary>
public class RelayManager : MonoBehaviour
{
    private const int MaxConnections = 2;
    private const string DefaultRoom = "DefaultRoom";

    private string joinCode; 

    async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Unity Services initialized and authenticated.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
        }
    }

    public async Task HostGame()
    {
        try
        {
            // Create a Relay allocation
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Hosting game. Join code: {joinCode}");

            // Start the host with the allocation
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to host game: {ex.Message}");
        }
    }

    public async Task JoinGame()
    {
        try
        {
            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("Join code is empty. Unable to join.");
                return;
            }

            // Join the Relay allocation
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("Successfully joined the game.");
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join game: {ex.Message}");
        }
    }
}