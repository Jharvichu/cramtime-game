using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private float refreshLobbyListTimer;
    private float heartbeatTimer;
    private Lobby joinedLobby;

    private string playerName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Authenticate();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    public async void Authenticate()
    {
        try
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetOption("Enviroment", "staging");

            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            await AuthenticationService.Instance.UpdatePlayerNameAsync(NetworkConfig.PLAYER_NAME_DEFAULT);

            this.playerName = AuthenticationService.Instance.PlayerName;

            Debug.Log("Welcome " + playerName);

        } catch (AuthenticationException e)
        {
            Debug.Log(e);
        }
        
    }

    public async Task CreateLobby()
    {
        try
        {
            Player player = CreatePlayerObject();

            string relayCode = await RelayManager.Instance.CreateRelay();

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = NetworkConfig.IS_ROOM_PRIVATE_DEFAULT,
                Data = new Dictionary<string, DataObject>
                {
                    { NetworkConfig.KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(NetworkConfig.DEFAULT_ROOM_NAME, NetworkConfig.PLAYER_COUNT_DEFAULT, options);

            // Evento

            Debug.Log("Created " + joinedLobby.Name);
            Debug.Log("Code: " + joinedLobby.LobbyCode);

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async Task JoinLobbyByCode(string lobbyCode)
    {
        Player player = CreatePlayerObject();

        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = player
        };

        joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);

        await RelayManager.Instance.JoinRelay(joinedLobby.Data[NetworkConfig.KEY_RELAY_CODE].Value);

        // Evento

        Debug.Log("Joined " + joinedLobby.Name);
    }

    public async Task JoinLobby(Lobby lobby)
    {
        Player player = CreatePlayerObject();

        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
        {
            Player = player
        };

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, options);

        await RelayManager.Instance.JoinRelay(joinedLobby.Data[NetworkConfig.KEY_RELAY_CODE].Value);

        // Evento

        Debug.Log("Joined " + lobby.Name);
    }

    public async Task<List<Lobby>> GetLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions {
                Count = NetworkConfig.DEFAULT_SERVER_COUNT,
                SampleResults = NetworkConfig.SAMPLE_RESULTS_DEFAULT,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0", QueryFilter.OpOptions.GT)  // Filtro para espacios libres mayor que cero
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)                                  // Ordenados de forma descendiente por creacion
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);

            return queryResponse.Results;

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    // Actualizacion

    public async void UpdatePlayerName(string playerName)
    {
        this.playerName = playerName;

        if(joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject> 
                    {
                        { NetworkConfig.KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                    }
                };

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, options);

                joinedLobby = lobby;

                // Evento

            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

    }

    // Refresh

    private void HandleRefreshLobbyList()
    {
        if(UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0)
            {
                refreshLobbyListTimer = NetworkConfig.LOBBY_REFRESH_RATE;
                // Refresh
                
            }
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0)
            {
                heartbeatTimer = NetworkConfig.HEARTBEAT_INTERVAL;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    // Utils

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private Player CreatePlayerObject()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { NetworkConfig.KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { NetworkConfig.KEY_PLAYER_DIFFICULTY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, GodparentStatus.Independiente.ToString()) }
        });
    }

}
