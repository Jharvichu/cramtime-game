using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite marineSprite;
    [SerializeField] private Sprite ninjaSprite;
    [SerializeField] private Sprite zombieSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManagers.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManagers.PlayerCharacter.Marine:   return marineSprite;
            case LobbyManagers.PlayerCharacter.Ninja:    return ninjaSprite;
            case LobbyManagers.PlayerCharacter.Zombie:   return zombieSprite;
        }
    }

}