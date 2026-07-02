using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class PlayerCanvas : MonoBehaviour
{
    public PausePanel pausePanel;

    private Player _player;
    private InGame _inGame =>InGame.Instance;
    public Camera playerCamera;

    public bool isPause {  get; private set; }

    public void Init(Player player, List<AllBuffCard> choseBuffs)
    {
        _player = player;
        pausePanel.Init(player, choseBuffs);
    }
    public void Button_Pause()
    {
        isPause = !isPause;
        pausePanel.gameObject.SetActive(isPause); 
    }

    public void TurnCamera(CameraStage cameraStage) => 
        StartCoroutine(_inGame.TurnCamera(playerCamera, _player.usingChess, cameraStage));
}
