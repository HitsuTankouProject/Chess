using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PoolObject : MonoBehaviour
{
    public Pool pool {  get; private set; }
    public void AddPool(Pool targetPool)
    {
        pool = targetPool;
    }
}

/// <summary>
/// プー?として管?するオブジェクトの情報を保?するク?ス。
/// MonoBehaviour を継承していないため、Inspector で表示・編集可能にするために
/// [System.Serializable] 属性を付与している。
/// </summary>
[System.Serializable]
public class Pool
{
    /// <summary>
    /// プー?の対象オブジェクト
    /// </summary>
    public GameObject Prefab => prefab;
    /// <summary>
    /// プー?のサイズ（?期設定サイズ）
    /// </summary>
    public int Size => size;
    /// <summary>
    /// 実行?のプー?内のオブジェクト?
    /// </summary>
    public int RuntimeSize => queue.Count;
    /// <summary>
    /// プー?内で使うオブジェクトのPrefab
    /// </summary>
    [SerializeField] private GameObject prefab;
    /// <summary>
    /// プー?の?期サイズ
    /// </summary>
    [SerializeField] private int size = 1;
    /// <summary>
    /// プー?として使用するオブジェクトのキ?ー。
    /// 現在使えるプー?内のオブジェクトを管?する。
    /// List<> より Queue<> を使用する?由は、
    /// ?頻度でのアクセスにおいて「先入れ先出し (FIFO)」が?速で?るため。
    /// プー?ではオブジェクトの取得・返却が頻繁に行われるため、 Queue<> は最も適したデータ構造と考えられる。
    /// </summary>
    private Queue<GameObject> queue;
    /// <summary>
    /// プー?で管?するオブジェクトの親として使用する Transform。
    /// オブジェクトを整?し、Hierarchy 上でまとめて管?するためのもの。
    /// </summary>
    private Transform parent;

    /// <summary>
    /// プー?の?期化を行う?ソッド。
    /// 指定された親オブジェクトの下にプー?用オブジェクトをまとめ、
    /// Queue に追加して管?可能にする。
    /// </summary>
    /// <param name="parent">プー?内オブジェクトをまとめる親の Transform</param>
    public void Initialize(Transform parent)
    {
        queue = new Queue<GameObject>();    // Queue を?期化して、プー?内のオブジェクトを格納
        this.parent = parent;               // 親 Transform を設定（Hierarchy 上で整?するため）

        for (var i = 0; i < size; i++)      // 指定されたサイズ分だけオブジェクトを生成して Queue に追加
        {
            queue.Enqueue(Copy());          // Copy() で新しいイ?スタ?スを作成して追加
        }
    }

    /// <summary>
    /// プ?ハブのコピー（イ?スタ?ス）を作成する?ソッド。
    /// 作成したオブジェクトはプー?用として親 Transform の下に配置され、
    /// ?期状態では非アクティブに設定される。
    /// </summary>
    /// <returns>複製された GameObject</returns>
    GameObject Copy()
    {
        var copy = GameObject.Instantiate(prefab, parent);  // プ?ハブを複製し、親 Transform の下に配置
        copy.SetActive(false);                              // プー?用なので非アクティブに設定
        copy.AddComponent<PoolObject>().AddPool(this);
        return copy;

    }

    /// <summary>
    /// プー?内で使用可能なオブジェクトを取得する?ソッド。
    /// キ?ーに非アクティブのオブジェクトが?ればそれを返し、
    /// なければ新しくプ?ハブを複製して返す。
    /// 取得後はキ?ーに再度戻すことでプー?の循環を維?する。
    /// </summary>
    /// <returns>使用可能な GameObject</returns>
    GameObject AvailableObject()
    {
        GameObject availableObject = null;

        if (queue.Count > 0 && !queue.Peek().activeSelf)    // キ?ーにオブジェクトが?り、非アクティブなものを使用
        {
            availableObject = queue.Dequeue();              // 使用するオブジェクトを取り出す
        }
        else
        {
            availableObject = Copy();                       // プー?に使用可能オブジェクトがなければ新規作成
        }

        queue.Enqueue(availableObject);                     // 使用したオブジェクトを再度キ?ーに戻す

        return availableObject;
    }

    /// <summary>
    /// プー?から使用可能なオブジェクトを?備して返す?ソッド（位置指定なし）。
    /// 取得したオブジェクトはアクティブ化され、すぐに使用可能な状態になる。
    /// </summary>
    /// <returns>?備された GameObject</returns>
    public GameObject PreparedObject()
    {
        GameObject preparedObject = AvailableObject();     // プー?から使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする

        return preparedObject;
    }

    /// <summary>
    /// プー?から使用可能なオブジェクトを?備して返す?ソッド（位置指定?り）。
    /// 取得したオブジェクトはアクティブ化され、指定した位置に配置される。
    /// </summary>
    /// <param name="position">オブジェクトを配置する?ー?ド座標</param>
    /// <returns>?備された GameObject</returns>
    public GameObject PreparedObject(Vector3 position)
    {
        GameObject preparedObject = AvailableObject();     // プー?から使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする
        preparedObject.transform.position = position;      // 指定された位置に配置

        return preparedObject;
    }

    /// <summary>
    /// プー?から使用可能なオブジェクトを?備して返す?ソッド（位置と回転指定?り）。
    /// 取得したオブジェクトはアクティブ化され、指定した位置と回転で配置される。
    /// </summary>
    /// <param name="position">オブジェクトを配置する?ー?ド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <returns>?備された GameObject</returns>
    public GameObject PreparedObject(Vector3 position, Quaternion rotation)
    {
        GameObject preparedObject = AvailableObject();     // プー?から使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする

        preparedObject.transform.position = position;      // 指定された位置を設定
        preparedObject.transform.rotation = rotation;      // 指定された回転を設定

        return preparedObject;
    }

    /// <summary>
    /// プー?から使用可能なオブジェクトを?備して返す?ソッド（位置・回転・スケー?指定?り）。
    /// 取得したオブジェクトはアクティブ化され、指定した位置・回転・スケー?で配置される。
    /// </summary>
    /// <param name="position">オブジェクトを配置する?ー?ド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <param name="localScale">オブジェクトのスケー?</param>
    /// <returns>?備された GameObject</returns>
    public GameObject PreparedObject(Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        GameObject preparedObject = AvailableObject();     // プー?から使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする

        preparedObject.transform.position = position;      // 指定された位置を設定
        preparedObject.transform.rotation = rotation;      // 指定された回転を設定
        preparedObject.transform.localScale = localScale;  // 指定されたスケー?・大きさを設定

        return preparedObject;
    }

    /// <summary>
    /// 使用したオブジェクトをプー?に返却する?ソッド。
    /// Queue に戻すことで、再度使用可能な状態として管?する。
    /// </summary>
    /// <param name="gameObject">返却する GameObject</param>
    public void Return(GameObject gameObject)
    {
        gameObject.SetActive(false);
        queue.Enqueue(gameObject);                          // 使用後のオブジェクトをキ?ーに戻す
    }


}
