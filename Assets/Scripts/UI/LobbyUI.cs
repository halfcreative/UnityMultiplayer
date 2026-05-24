using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    private Button mainMenuButton;
    [SerializeField]
    private Button createLobbyButton;
    [SerializeField]
    private Button quickJoinButton;
    [SerializeField]
    private Button codeJoinButton;
    [SerializeField]
    private TMP_InputField lobbyCodeInputField;
    [SerializeField]
    private TMP_InputField playerNameInputField;
    [SerializeField]
    private LobbyCreateUI lobbyCreateUI;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });
        quickJoinButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.QuickJoin();
        });
        codeJoinButton.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);
        });

    }

    private void Start()
    {
        playerNameInputField.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string name) =>
        {
            KitchenGameMultiplayer.Instance.SetPlayerName(name);
        });
    }






}
