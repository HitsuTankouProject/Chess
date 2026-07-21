using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;

/// <summary>
/// カードの表示面を表します。
/// </summary>
public enum CardFace {
    /// <summary>カードの表面です。</summary>
    Front,
    /// <summary>カードの裏面です。</summary>
    Back
}

/// <summary>
/// バフカードの見た目と回転アニメーションを管理します。
/// 指定されたバフカードに対応する画像を表面へ設定し、
/// 必要に応じてカードのエフェクト表示を切り替えます。
/// また、表面と裏面の目標角度を計算し、一定時間をかけて
/// カードを滑らかに回転させる処理を担当します。
/// </summary>
public class Card : MonoBehaviour
{
    /// <summary>ゲーム全体を管理する共有インスタンスを取得します。</summary>
    private GameManager _gameManager => GameManager.Instance;

    /// <summary>このオブジェクトに設定されているバフカードです。</summary>
    public AllBuffCard buffCard;

    /// <summary>カード表面を描画するレンダラーです。</summary>
    public MeshRenderer card_Front;
    /// <summary>カード裏面を描画するレンダラーです。</summary>
    public MeshRenderer card_back;
    /// <summary>カードの特殊効果を描画するレンダラーです。</summary>
    public MeshRenderer card_effectPrt;

    /// <summary>
    /// 表示するバフカードとエフェクトの状態を設定します。
    /// カード表面には、現在の言語設定に対応するカバー画像を適用します。
    /// </summary>
    /// <param name="card">このオブジェクトへ設定するバフカードです。</param>
    /// <param name="isOpenEffect">カードエフェクトを表示する場合は <see langword="true" /> です。</param>
    /// <param name="effectLevel">表示するエフェクトのレベルです。</param>
    public void SetCard(AllBuffCard card, bool isOpenEffect = false, uint effectLevel = 0)
    {
        buffCard = card;
        card_Front.material = LanguageManager.Instance.cardDataDict[buffCard].m_CardCover;
        if (!isOpenEffect) card_effectPrt.enabled = false;
        else
        {
            card_effectPrt.enabled = true;
            //card_effectPrt.material=
        }


    }

    /// <summary>カードを表裏に回転させる時間（秒）です。</summary>
    private const float cardTurnTime = 0.35f;

    /// <summary>
    /// 指定された表示面に対応する Y 軸の最終角度を取得します。
    /// </summary>
    /// <param name="faceTo">回転後に表示するカード面です。</param>
    /// <returns>表面の場合は 0 度、裏面の場合は 180 度を返します。</returns>
    private float FinalFaceTo(CardFace faceTo) => faceTo == CardFace.Front ? 0f : 180f;
    /// <summary>
    /// 指定された面が表示されるようにカードを滑らかに回転させます。
    /// すでに目標角度に到達している場合は、回転処理を行いません。
    /// </summary>
    /// <param name="faceTo">回転後に表示するカード面です。</param>
    public async UniTask TurnTheCard(CardFace faceTo)
    {
        // 表示する面から目標角度を求め、現在角度を開始位置として保持します。
        float targetY = FinalFaceTo(faceTo);
        float startY = transform.localEulerAngles.y;
        // 角度差が十分に小さい場合は、不要なアニメーションを省略します。
        if (Mathf.Abs(Mathf.DeltaAngle(startY, targetY)) < 0.1f) return;

        float elapsedTime = 0f;
        // 経過時間の割合に合わせ、最短方向へ角度を補間します。
        while (elapsedTime < cardTurnTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / cardTurnTime);

            float y = Mathf.LerpAngle(startY, targetY, t);

            transform.localEulerAngles = new Vector3(0, y, 0);

            await UniTask.Yield();
        }
        // 補間誤差が残らないよう、最後に目標角度を明示的に設定します。
        transform.localEulerAngles = new Vector3(0, targetY, 0);


    }


}
