using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerCanvas : MonoBehaviour
{
    public PausePanel pausePanel;
    private Player _player;
    private bool isPause = false;

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



}
