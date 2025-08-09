using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GnerealSessionManager : MonoBehaviour
{
    public Camera Player1Camera;
    public Camera GameMasterCamera;
    public ObjectPlacer objectPlacer;
    public LayerMask player1MapGroundLayer;
    public LayerMask gameMasterGroundLayer;
    public UiControllerCreateSessionGameScene uiControllerCreateSessionGameScene;
    public PlayerMapLoader playerMapLoader;
    public Canvas Player1Canvas;
    public Canvas GameMasterCanvas;
    public RadioButtonUI teamChangeRadioButton;

    private bool reloadFlag;
    private int playerAmmount;

    void Start()
    {
        // Start in Game Master view
        switchToGameMasterScene();
        reloadFlag = false;

        playerAmmount = SceneData.playerAmmount;
        SceneData.playerAmmount = 0;

        SetPlayerOptions();
    }
    void SetPlayerOptions()
    {
        Debug.Log(playerAmmount);
        if (teamChangeRadioButton == null)
        {
            Debug.Log("RadioButtonUI reference is missing!");
            return;
        }

        string[] options = new string[playerAmmount + 1];
        options[0] = "All";

        for (int i = 1; i <= playerAmmount; i++)
        {
            options[i] = $"Player {i}";
        }

        teamChangeRadioButton.SetOptions(options);
    }

    public int getPlayerAmmount()
    {
        return playerAmmount;
    }

    public void setReloadFlagTrue()
    {
        reloadFlag = true;
    }

    public void switchToPlayer1Scene()
    {
        if (reloadFlag)
        {
            uiControllerCreateSessionGameScene.hitSaveButton();
            playerMapLoader.ReloadMap();
            reloadFlag = false;
        }
        // Disable Game Master camera and canvas
        GameMasterCamera.enabled = false;
        GameMasterCanvas.gameObject.SetActive(false);
        GameMasterCamera.gameObject.tag = "Untagged";

        // Enable Player 1 camera and canvas
        Player1Camera.enabled = true;
        Player1Canvas.gameObject.SetActive(true);
        Player1Camera.gameObject.tag = "MainCamera";

        // Set placement layer
        objectPlacer.setGroundLayerForPlacement(player1MapGroundLayer);
    }

    public void switchToGameMasterScene()
    {
        // Disable Player 1 camera and canvas
        Player1Camera.enabled = false;
        Player1Canvas.gameObject.SetActive(false);
        Player1Camera.gameObject.tag = "Untagged";

        // Enable Game Master camera and canvas
        GameMasterCamera.enabled = true;
        GameMasterCanvas.gameObject.SetActive(true);
        GameMasterCamera.gameObject.tag = "MainCamera";

        // Set placement layer
        objectPlacer.setGroundLayerForPlacement(gameMasterGroundLayer);
    }
}