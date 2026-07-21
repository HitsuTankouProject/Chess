/// <summary>
/// すべての駒バフに共通する基底クラスです。
/// バフの対象駒、表示名、選択時の処理、レベルごとの効果を
/// 派生クラスへ定義させ、現在レベルの管理と初期化処理を提供します。
/// バフレベルは0から3までで、各レベルへ到達した際に対応する
/// <see cref="FirstLevel" />、<see cref="SecondLevel" />、
/// <see cref="ThirdLevel" /> を呼び出します。
/// </summary>
public abstract class BuffBasic
{
    /// <summary>このバフを適用できる駒種を取得します。</summary>
    public abstract ChessType buffChess { get; }
    /// <summary>このバフを所有するプレイヤーを取得します。</summary>
    public Player _player {  get; private set; }
    /// <summary>プレイヤーがこのバフを選択した際の状態変更を行います。</summary>
    public abstract void Choose();
    /// <summary>バフの表示名を取得します。</summary>
    public abstract string buffName { get; }
    /// <summary>現在のバフレベルを取得します。未取得時は0です。</summary>
    public uint nowBuffLevel { get; private set; } = 0;
    /// <summary>バフが到達できる最大レベルです。</summary>
    private const uint maxBuffLevel = 3;

    /// <summary>派生クラス固有の状態を初期状態へ戻します。</summary>
    public abstract void ResetBuff();
    /// <summary>レベル1で開放される効果を適用します。</summary>
    public abstract void FirstLevel();
    /// <summary>レベル2で開放される効果を適用します。</summary>
    public abstract void SecondLevel();
    /// <summary>レベル3で開放される効果を適用します。</summary>
    public abstract void ThirdLevel();

        /// <summary>
    /// 現在のバフレベルを1段階上げ、到達したレベルの効果を適用します。
    /// 最大レベルに到達済みの場合は、現在の状態を変更しません。
    /// </summary>
    /// <param name="success">レベルアップに成功した場合は <see langword="true" /> です。</param>
    public virtual void LevelUp(out bool success)
    {
        // レベルが有効範囲外の場合は処理を中止します。
        if (nowBuffLevel >= maxBuffLevel || nowBuffLevel < 0) 
        {
            success = false;
            return;
        }
        nowBuffLevel++;
        // 新しく到達したレベルに対応する固有効果を適用します。
        switch (nowBuffLevel)
        {
            case 1:FirstLevel();break;
            case 2:SecondLevel();break;
            case 3:ThirdLevel();break;
            default: success = false; return;
        }
        success = true;
    }
    /// <summary>
    /// バフを指定レベルへ直接設定し、そのレベルまでの効果を順番に適用します。
    /// レベル0を指定した場合は、バフ固有の状態をリセットします。
    /// </summary>
    /// <param name="targetLevel">設定するバフレベルです。0から3まで指定できます。</param>
    /// <param name="success">指定レベルの設定に成功した場合は <see langword="true" /> です。</param>
    public virtual void LevelUpToTargetLevel(uint targetLevel, out bool success)
    {
        if(targetLevel > maxBuffLevel)
        {
            success = false;
            return;
        }

        nowBuffLevel = targetLevel;
        // 指定レベルまでに開放される効果を低いレベルから順に適用します。
        switch (nowBuffLevel)
        {
            case 0:
                ResetBuff();
                break;
            case 1: 
                FirstLevel(); 
                break;
            case 2:
                FirstLevel();
                SecondLevel();
                break;
            case 3:
                FirstLevel();
                SecondLevel();
                ThirdLevel(); 
                break;

            default: success = false; return;


        }
        success = true;

    }

    /// <summary>
    /// バフレベルと固有状態をリセットし、所有者となるプレイヤーを設定します。
    /// </summary>
    /// <param name="player">このバフを所有するプレイヤーです。</param>
    public virtual void BuffInit(Player player)
    {
        // レベルと派生クラス固有の状態を初期値へ戻します。
        nowBuffLevel = 0;
        ResetBuff();
        _player = player;
    }

}
