using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using IngameDebugConsole;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HelloWorld
{
    public class TestLobby : MonoBehaviour
    {

        private static Lobby hostLobby;
        private float heartbeatTimer;
        private static string playerName;

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            playerName = "Jharvichu" + Random.Range(10, 99);
            Debug.Log("Bienvenido jugador : " + playerName);
        }

        private void Update()
        {
            HandleLobbyHearbeat();
        }

        private async void HandleLobbyHearbeat()
        {
            if (hostLobby == null)
                return;

            if (hostLobby.HostId != AuthenticationService.Instance.PlayerId)
                return;

            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }

        }

        [ConsoleMethod("create_lobby", "Crea un lobby: nombre maxJugadores")]
        public static async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
        {
            try
            {
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                            { "PlayerType", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Ahijado") }
                        }
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                Debug.Log("Lobby creado! " + lobby.Name + " con " + lobby.MaxPlayers + " jugadores y codigo " + lobby.LobbyCode);
                hostLobby = lobby;
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        [ConsoleMethod("list_lobbies", "Lista los lobbies")]
        public static async void ListLobbies()
        {
            try
            {
                QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
                Debug.Log("hay " + queryResponse.Results.Count + " lobbys");

                foreach (Lobby lobby in queryResponse.Results)
                {
                    Debug.Log("Lobby: " + lobby.Name + " con " + lobby.MaxPlayers + " jugadores");
                }

            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        [ConsoleMethod("join_lobby", "Unirse a un lobby mediante código")]
        public static async void JoinLobbyByCode(string lobbyCode)
        {
            try 
            {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions 
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                            { "PlayerType", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Ahijado") }
                        }
                    }
                };

                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
                Debug.Log("Te has unido al lobby: " + lobby.Name + " usando el código " + lobbyCode);
                hostLobby = lobby;
            } catch (LobbyServiceException e) 
            {
                print (e);
            }
        }

        [ConsoleMethod("quick_join_lobby", "Unirse a un lobby rapidamente")]
        public static async void QuickJoinLobby()
        {
            try
            {
                QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                            { "PlayerType", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Ahijado") }
                        }
                    }
                };

                await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                print(e);
            }
        }

        [ConsoleMethod("show_players", "Ver jugadores del lobby")]
        public static async void ShowPlayer()
        {
            try
            {
                if (hostLobby == null) return;
                hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                Debug.Log("Players en el lobby " + hostLobby.Name);
                foreach (Player player in hostLobby.Players)
                {
                    Debug.Log("Nombre: " + player.Data["PlayerName"].Value + ", Tipo: " + player.Data["PlayerType"].Value + " (" + player.Id + ")");
                }
            }
            catch (LobbyServiceException e) 
            {
                print(e);
            }
        }
    }    
}


