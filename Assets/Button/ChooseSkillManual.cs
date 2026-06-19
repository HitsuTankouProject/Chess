using UnityEngine;

public class ChooseSkillManual : ButtonManual
{

    public override void PickTheCard(Card card)
    {
        Debug.Log("ChooseSkillManual: PickTheCard");
    }

    public override void Return()
    {
        Debug.Log("ChooseSkillManual: Return");
    }



}
