using QFSW.QC;
using QFSW.QC.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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
        } catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }


    private void PrintPlayers(Lobby lobby)
    {
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
}
