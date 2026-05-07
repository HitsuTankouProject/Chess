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
/// プールとして管理するオブジェクトの情報を保持するクラス。
/// MonoBehaviour を継承していないため、Inspector で表示・編集可能にするために
/// [System.Serializable] 属性を付与している。
/// </summary>
[System.Serializable]
public class Pool
{
    /// <summary>
    /// プールの対象オブジェクト
    /// </summary>
    public GameObject Prefab => prefab;
    /// <summary>
    /// プールのサイズ（初期設定サイズ）
    /// </summary>
    public int Size => size;
    /// <summary>
    /// 実行時のプール内のオブジェクト数
    /// </summary>
    public int RuntimeSize => queue.Count;
    /// <summary>
    /// プール内で使うオブジェクトのPrefab
    /// </summary>
    [SerializeField] private GameObject prefab;
    /// <summary>
    /// プールの初期サイズ
    /// </summary>
    [SerializeField] private int size = 1;
    /// <summary>
    /// プールとして使用するオブジェクトのキュー。
    /// 現在使えるプール内のオブジェクトを管理する。
    /// List<> より Queue<> を使用する理由は、
    /// 高頻度でのアクセスにおいて「先入れ先出し (FIFO)」が高速であるため。
    /// プールではオブジェクトの取得・返却が頻繁に行われるため、 Queue<> は最も適したデータ構造と考えられる。
    /// </summary>
    private Queue<GameObject> queue;
    /// <summary>
    /// プールで管理するオブジェクトの親として使用する Transform。
    /// オブジェクトを整理し、Hierarchy 上でまとめて管理するためのもの。
    /// </summary>
    private Transform parent;

    /// <summary>
    /// プールの初期化を行うメソッド。
    /// 指定された親オブジェクトの下にプール用オブジェクトをまとめ、
    /// Queue に追加して管理可能にする。
    /// </summary>
    /// <param name="parent">プール内オブジェクトをまとめる親の Transform</param>
    public void Initialize(Transform parent)
    {
        queue = new Queue<GameObject>();    // Queue を初期化して、プール内のオブジェクトを格納
        this.parent = parent;               // 親 Transform を設定（Hierarchy 上で整理するため）

        for (var i = 0; i < size; i++)      // 指定されたサイズ分だけオブジェクトを生成して Queue に追加
        {
            queue.Enqueue(Copy());          // Copy() で新しいインスタンスを作成して追加
        }
    }

    /// <summary>
    /// プレハブのコピー（インスタンス）を作成するメソッド。
    /// 作成したオブジェクトはプール用として親 Transform の下に配置され、
    /// 初期状態では非アクティブに設定される。
    /// </summary>
    /// <returns>複製された GameObject</returns>
    GameObject Copy()
    {
        var copy = GameObject.Instantiate(prefab, parent);  // プレハブを複製し、親 Transform の下に配置
        copy.SetActive(false);                              // プール用なので非アクティブに設定
        copy.AddComponent<PoolObject>().AddPool(this);
        return copy;

    }

    /// <summary>
    /// プール内で使用可能なオブジェクトを取得するメソッド。
    /// キューに非アクティブのオブジェクトがあればそれを返し、
    /// なければ新しくプレハブを複製して返す。
    /// 取得後はキューに再度戻すことでプールの循環を維持する。
    /// </summary>
    /// <returns>使用可能な GameObject</returns>
    GameObject AvailableObject()
    {
        GameObject availableObject = null;

        if (queue.Count > 0 && !queue.Peek().activeSelf)    // キューにオブジェクトがあり、非アクティブなものを使用
        {
            availableObject = queue.Dequeue();              // 使用するオブジェクトを取り出す
        }
        else
        {
            availableObject = Copy();                       // プールに使用可能オブジェクトがなければ新規作成
        }

        queue.Enqueue(availableObject);                     // 使用したオブジェクトを再度キューに戻す

        return availableObject;
    }

    /// <summary>
    /// プールから使用可能なオブジェクトを準備して返すメソッド（位置指定なし）。
    /// 取得したオブジェクトはアクティブ化され、すぐに使用可能な状態になる。
    /// </summary>
    /// <returns>準備された GameObject</returns>
    public GameObject PreparedObject()
    {
        GameObject preparedObject = AvailableObject();     // プールから使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする

        return preparedObject;
    }

    /// <summary>
    /// プールから使用可能なオブジェクトを準備して返すメソッド（位置指定あり）。
    /// 取得したオブジェクトはアクティブ化され、指定した位置に配置される。
    /// </summary>
    /// <param name="position">オブジェクトを配置するワールド座標</param>
    /// <returns>準備された GameObject</returns>
    public GameObject PreparedObject(Vector3 position)
    {
        GameObject preparedObject = AvailableObject();     // プールから使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする
        preparedObject.transform.position = position;      // 指定された位置に配置

        return preparedObject;
    }

    /// <summary>
    /// プールから使用可能なオブジェクトを準備して返すメソッド（位置と回転指定あり）。
    /// 取得したオブジェクトはアクティブ化され、指定した位置と回転で配置される。
    /// </summary>
    /// <param name="position">オブジェクトを配置するワールド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <returns>準備された GameObject</returns>
    public GameObject PreparedObject(Vector3 position, Quaternion rotation)
    {
        GameObject preparedObject = AvailableObject();     // プールから使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする

        preparedObject.transform.position = position;      // 指定された位置を設定
        preparedObject.transform.rotation = rotation;      // 指定された回転を設定

        return preparedObject;
    }

    /// <summary>
    /// プールから使用可能なオブジェクトを準備して返すメソッド（位置・回転・スケール指定あり）。
    /// 取得したオブジェクトはアクティブ化され、指定した位置・回転・スケールで配置される。
    /// </summary>
    /// <param name="position">オブジェクトを配置するワールド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <param name="localScale">オブジェクトのスケール</param>
    /// <returns>準備された GameObject</returns>
    public GameObject PreparedObject(Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        GameObject preparedObject = AvailableObject();     // プールから使用可能なオブジェクトを取得
        preparedObject.SetActive(true);                    // オブジェクトをアクティブにして使用可能にする

        preparedObject.transform.position = position;      // 指定された位置を設定
        preparedObject.transform.rotation = rotation;      // 指定された回転を設定
        preparedObject.transform.localScale = localScale;  // 指定されたスケール・大きさを設定

        return preparedObject;
    }

    /// <summary>
    /// 使用したオブジェクトをプールに返却するメソッド。
    /// Queue に戻すことで、再度使用可能な状態として管理する。
    /// </summary>
    /// <param name="gameObject">返却する GameObject</param>
    public void Return(GameObject gameObject)
    {
        gameObject.SetActive(false);
        queue.Enqueue(gameObject);                          // 使用後のオブジェクトをキューに戻す
    }


}
