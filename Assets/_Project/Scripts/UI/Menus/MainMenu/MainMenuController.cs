using System;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private UIDocument _doc;
    private Button _btnCreate, _btnJoin, _btnOption, _btnQuit, _btnUpdateName;
    private TextField _playerNameField;

    [SerializeField] private GameObject JoinGamePanel;
    [SerializeField] private GameObject OptionsPanel;

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
        _btnCreate = root.Q<Button>("BtnCreate");
        _btnJoin = root.Q<Button>("BtnJoin");
        _btnOption = root.Q<Button>("BtnSettings");
        _btnQuit = root.Q<Button>("BtnQuit");
        _btnUpdateName = root.Q<Button>("BtnUpdateName");

        _playerNameField = root.Q<TextField>("InputPlayerName");
    }

    private void UnregisterEvents()
    {
        if (_btnCreate != null) _btnCreate.clicked -= CreateLobby;
        if (_btnJoin != null) _btnJoin.clicked -= OpenJoinGamePanel;
        if (_btnOption != null) _btnOption.clicked -= OpenOptionsPanel;
        if (_btnUpdateName != null) _btnUpdateName.clicked -= UpdateNamePlayer;
        if (_btnQuit != null) _btnQuit.clicked -= QuitGame;

    }

    private void RegisterEvents()
    {
        UnregisterEvents();

        _btnCreate.clicked += CreateLobby;
        _btnJoin.clicked += OpenJoinGamePanel;
        _btnOption.clicked += OpenOptionsPanel;
        _btnUpdateName.clicked += UpdateNamePlayer;
        _btnQuit.clicked += QuitGame;

    }

    private async void CreateLobby()
    {
        _btnCreate.SetEnabled(false);

        try
        {
            await LobbyManager.Instance.CreateLobby();

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);

        } catch (Exception e)
        {
            Debug.Log(e);
            _btnCreate.SetEnabled(true);
        }
        
    }

    private void OpenJoinGamePanel()
    {

        _btnJoin.SetEnabled(false);

        if (JoinGamePanel != null)
        {
            JoinGamePanel.SetActive(true);
            gameObject.SetActive(false);

            Debug.Log("Cambio al panel JoinGame realizado con éxito.");
        }
    }

    private void OpenOptionsPanel()
    {

    }

    private void UpdateNamePlayer()
    {
        if(!string.IsNullOrWhiteSpace(_playerNameField.text))
        {
            LobbyManager.Instance.UpdatePlayerName(_playerNameField.text.Trim());
            Debug.Log("Nombre Actualizado a " + _playerNameField.text.Trim());
        }
    }

    private void QuitGame()
    {

    }

}
