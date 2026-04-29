using UnityEngine;
using UnityEngine.UI;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Game button clicked");
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });

        joinGameButton.onClick.AddListener(() =>
        {
            Debug.Log("Join Game button clicked");
            KitchenGameMultiplayer.Instance.StartClient();
            // Client will automatically load the scene to match the host - no need to load the scene here
        });
    }


}
