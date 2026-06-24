using Unity.VisualScripting;
using UnityEngine;

public class PauseManual : ButtonManual
{

    private InGame _inGame => InGame.Instance;
    private Player white => _inGame.whiteChessPlayer;
    private Player black => _inGame.blackChessPlayer;


    public Card[] whiteBuffs;
    public Card[] blackBuffs;



    public override void Pause()
    {
        Debug.Log("PauseManual: Pause");
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public override void Return()
    {
        Debug.Log("PauseManual: Return");
    }


    

}
