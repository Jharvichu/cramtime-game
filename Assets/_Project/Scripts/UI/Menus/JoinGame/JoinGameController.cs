using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class JoinGameController : MonoBehaviour
{
    private UIDocument _doc;
    private Button _btnJoinWithCode, _btnClose, _btnRefresh;
    private TextField _inputCodeField;
    private ScrollView _serverListScroll;

    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private VisualTreeAsset serverItemTemplate;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
    }

    private void Start()
    {

    }

    void OnEnable()
    {
        VisualElement root = _doc.rootVisualElement;
        FindUIElements(root);
        RegisterEvents();
    }

    void OnDisable()
    {
        UnregisterEvents();
    }

    private void FindUIElements(VisualElement root)
    {
        _btnJoinWithCode = root.Q<Button>("BtnJoinWithCode");
        _btnClose = root.Q<Button>("BtnClose");
        _btnRefresh = root.Q<Button>("BtnRefresh");

        _inputCodeField = root.Q<TextField>("InputCode");

        _serverListScroll = root.Q<ScrollView>("ServerList");
    }

    private void UnregisterEvents()
    {
        if (_btnJoinWithCode != null) _btnJoinWithCode.clicked -= JoinLobbyByCode;
        if (_btnRefresh != null) _btnRefresh.clicked -= RefreshLobbiesList;
        if (_btnClose != null) _btnClose.clicked -= BackToMainMenu;
    }

    private void RegisterEvents()
    {
        UnregisterEvents();

        _btnJoinWithCode.clicked += JoinLobbyByCode;
        _btnRefresh.clicked += RefreshLobbiesList;
        _btnClose.clicked += BackToMainMenu;

    }

    private async void JoinLobbyByCode()
    {
        _btnJoinWithCode.SetEnabled(false);

        if (!string.IsNullOrWhiteSpace(_inputCodeField.text))
        {
            try
            {
                await LobbyManager.Instance.JoinLobbyByCode(_inputCodeField.text.Trim());

                NetworkManager.Singleton.StartClient();

            }
            catch (Exception e)
            {
                Debug.Log(e);
                _btnJoinWithCode.SetEnabled(true);
            }
        }
    }

    private async void JoinLobby(Lobby lobby)
    {
        try
        {
            await LobbyManager.Instance.JoinLobby(lobby);
            NetworkManager.Singleton.StartClient();
        } catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async void RefreshLobbiesList()
    {
        _serverListScroll.Clear();

        try
        {
            List<Lobby> lobbies = await LobbyManager.Instance.GetLobbies();

            foreach(Lobby lobby in lobbies)
            {
                VisualElement item = serverItemTemplate.Instantiate();

                item.Q<Label>("ServerName").text = lobby.Name;
                item.Q<Label>("ServerStats").text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

                Button joinBtn = item.Q<Button>("BtnJoinServer");

                joinBtn.clicked += () =>
                {
                    joinBtn.SetEnabled(false);
                    Debug.Log($"Uniendo a: {lobby.Name}");
                    JoinLobby(lobby);
                };

                _serverListScroll.Add(item);
            }


        } catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    private void BackToMainMenu()
    {
        _btnClose.SetEnabled(false);

        if (MainMenuPanel != null)
        {
            MainMenuPanel.SetActive(true);
            gameObject.SetActive(false);

            Debug.Log("Cambio al panel MainMenu realizado con éxito.");
        }
    }


}
