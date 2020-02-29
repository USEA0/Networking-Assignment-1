using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonData : MonoBehaviour
{
    public PlayerProfile selfProfile;
    public Text selfText;

    public void Awake()
    {
        selfText = this.GetComponentInChildren<Text>();
    }

    public void Setup(PlayerProfile profile) {
        selfProfile = profile;

        if (selfProfile.inGame) {
            selfText.text = selfProfile.username + ": In-Game";
        }
        else
        {
            selfText.text = selfProfile.username + ": Avaliable";
        }
    }

    public void OnClick() {
        NetworkManager.RequestGame(selfProfile.id);
        UIManager.Instance.LobbyLog.text += "Requesting Game from player: " + selfProfile.username;
    }
}
