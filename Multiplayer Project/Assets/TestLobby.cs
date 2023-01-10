using QFSW.QC;
using QFSW.QC.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{


    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName = "Player";
    


    private async void Start()
    {
        playerName += UnityEngine.Random.Range(0, 101);


        // Random profile to test multiple instances on one device without sending too many requests
        string randomProfile = Guid.NewGuid().ToString();
        InitializationOptions initializationOptions = new InitializationOptions().SetProfile(randomProfile);
        await UnityServices.InitializeAsync(initializationOptions);


        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    [SerializeField] TMP_Text lobbyCode;

    [Command]
    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            lobbyCode.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();


            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayKey", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, updateLobbyOptions);

        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinRelay()
    {
        try
        {
            string joinCode = joinedLobby.Data["RelayKey"].Value;
            Debug.Log("Joining relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    [Command]
    private async Task CreateLobby()
    {
        string lobbyName = "MyLobby";
        int maxPlayers = 4;
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;

            Debug.Log("Created lobby: " + lobby.Name + ", " + lobby.MaxPlayers + " , " + lobby.LobbyCode);

            PrintPlayers(lobby);


            CreateRelay();

            
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " / " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();


            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            PrintPlayers(lobby);

            JoinRelay();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }


    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            joinedLobby = lobby;
            PrintPlayers(lobby);

            JoinRelay();
        } catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    private void PrintPlayers(Lobby lobby = null)
    {
        if (lobby == null)
        {
            lobby = joinedLobby;
        }
        Debug.Log("Players in lobby " + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private Player GetPlayer()
    {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    [Command]
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newPlayerName)
                }
            }
            });
        } catch(Exception e)
        {
            Debug.LogException(e);
        }
        
    }

    [Command]
    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void KickPlayer(string playerId)
    {
        // Implement later
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    [Command]
    private async void MigrateLobbyHost()
    {
        // Implement later
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;

        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        } catch(Exception e)
        {
            Debug.LogException(e);
        }
    }
}
