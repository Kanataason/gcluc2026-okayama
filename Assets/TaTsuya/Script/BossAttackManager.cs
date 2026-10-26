using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static BossBehaviorManager;
public class BossAttackManager : MonoBehaviour
{
    public static readonly int AttackHash = Animator.StringToHash("Attack");
    public static readonly int AttackTypeHash = Animator.StringToHash("AttackType");
    public static readonly int TeleportHash = Animator.StringToHash("Teleport");
    public static readonly int TransparentHash = Animator.StringToHash("Transparent");
    public static readonly int HideHash = Animator.StringToHash("Hide");
    private const float Attack1StartTime = 1.2f;//召喚した弾が攻撃を始めるまでの時間
    private const float Attack2Interval = 1f;//Attack2の発射間隔
    private const float Attack2HeightRate = 1.6f;//Attack2の弾の高さ(ステージ下端からの割合)
    private const float Attack3Offset = 3f;//Attack3でプレイヤーからどれだけ離れて出るか

    Animator a_Animator;

    private readonly List<BossBulletManager> l_BulletList = new();
    [FormerlySerializedAs("t_SpownPos")]
    public Transform t_SpawnPos;
    [SerializeField] private SpriteRenderer r_SpriteRen;
    [SerializeField] private SpriteRenderer r_Shadow;

    public bool b_IsBossCoroutine = false;//Attack2,Attack3のコルーチン実行中か(falseにすると中断する)

    //参照先
    private BossBaseManager c_BossBaseManager;
    private PlayerMove c_PlayerMove;
    private ObjctPool c_ObjectPool;
    private Camera c_Camera;
    private AudioSource c_SeSource;//このボス(ステージ)専用の効果音 弾の音もここで鳴らす

    public void Init(InputInfo inputInfo, StageContext stage)
    {
        c_Camera = stage.c_Camera;
        c_SeSource = AudioManager.Instance.CreateSeSource(gameObject);
        GetComponents(stage);
    }
    void GetComponents(StageContext stage)
    {
        c_PlayerMove = stage.c_Player;
        c_ObjectPool = GetComponentInChildren<ObjctPool>();
        c_BossBaseManager = GetComponent<BossBaseManager>();
        a_Animator = GetComponent<Animator>();
    }
    public void AttackEnter(int attackType)
    {
        a_Animator.SetInteger(AttackTypeHash, attackType);
        a_Animator.SetTrigger(AttackHash);
        c_BossBaseManager.SetIsAttackFlag(true);
    }
    public void SpawnEfect(int EventType)//アニメーションイベント(名前変更不可) EventTypeはObjctPool.EfectType
    {
        GameObject obj = c_ObjectPool.GetObject(CharaState.Boss, (ObjctPool.EfectType)EventType);

        SortOrderManager.Instance.SetSortOrder(obj.GetComponent<Renderer>());//親(プール)の位置を使うので親を外す前に呼ぶ
        Vector3 pos = t_SpawnPos.position;
        SetBulletInfo(obj, new Vector3(pos.x, pos.y, transform.position.y));
    }

    //攻撃&セット処理
    private void SetBulletInfo(GameObject obj, Vector3 pos)//弾を配置して登録する
    {
        obj.transform.parent = null;
        obj.transform.position = pos;

        var bullet = obj.GetComponent<BossBulletManager>();
        l_BulletList.Add(bullet);
        bullet.OnDestroyObjEvent += DestroyInfoList;

        InitBulletByType(bullet);
    }
    private void InitBulletByType(BossBulletManager bullet)//攻撃の種類ごとに弾を初期化
    {
        switch (bullet.e_Attacktype)
        {
            case BossAttackType.Attack1://当たり判定はアニメーションイベントでONにする
                bullet.Init(Attack1StartTime, BossBulletManager.BulletState.Stop, c_PlayerMove, c_BossBaseManager, c_SeSource, false);
                break;
            case BossAttackType.Attack2:
                bullet.Init(0f, BossBulletManager.BulletState.Move, c_PlayerMove, c_BossBaseManager, c_SeSource);
                bullet.Move();
                break;
            default:
                bullet.Init(0f, BossBulletManager.BulletState.Destroy, c_PlayerMove, c_BossBaseManager, c_SeSource);
                break;
        }
    }
    private void DestroyInfoList(BossBulletManager bullet, int charaType, int efectType)
    {
        bullet.OnDestroyObjEvent -= DestroyInfoList;
        bullet.transform.parent = c_ObjectPool.transform;

        l_BulletList.Remove(bullet);
        //コルーチン中(Attack2の連射中など)は弾が一時的に0になっても攻撃を終わらせない
        if (l_BulletList.Count == 0 && !b_IsBossCoroutine)
            ResetAttackFlag();

        c_ObjectPool.ReturnObject((CharaState)charaType, (ObjctPool.EfectType)efectType, bullet.gameObject);
    }
    //--------------------ここからアニメーションの値参照
    public void ResetAttackFlag()//アニメーションイベント(名前変更不可)
    {
        a_Animator.SetInteger(AttackTypeHash, 0);
        c_BossBaseManager.SetIsAttackFlag(false);
        SetCoroutineFlag(false);
    }
    public void SetCoroutineFlag(bool isRunning) => b_IsBossCoroutine = isRunning;
    public void SetIsMove() => SetCoroutineFlag(true);//アニメーションイベント(名前変更不可)
    public void StopSe()//アニメーションイベント(名前変更不可) 自分のステージの音だけ止める
    {
        c_SeSource.Stop();
    }
    public void PlaySe(string name)//アニメーションイベント(名前変更不可)
    {
        AudioManager.Instance.PlaySeAudio(name, c_SeSource);
    }
    public void SetTransparent(bool isTransparent, bool isHide)
    {
        a_Animator.SetBool(TransparentHash, isTransparent);
        a_Animator.SetBool(HideHash, isHide);
    }
    public void PlayTeleportAnima()//接敵時の登場アニメーション
    {
        SetTransparent(true, false);
        a_Animator.SetTrigger(TeleportHash);
    }
    //----------------ここから攻撃処理の中身(アニメーションイベントから呼ばれる)
    public void Attack1(int instantiateValue)//召喚魔法
    {
        float width = GetCameraHalfWidth();
        float camX = c_Camera.transform.position.x;
        var gameManager = TatuGameManager.Instance;
        for (int i = 0; i < instantiateValue; i++)
        {
            float randomY = Random.Range(gameManager.m_StageScaleMinY, gameManager.m_StageScaleMaxY);
            float randomX = Random.Range(camX - width, camX + width);

            var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Shot);
            SetBulletInfo(obj, new Vector3(randomX, randomY, randomY));
        }
    }
    public void Attack2(int instantiateValue)//ジャンプが必要な攻撃
    {
        a_Animator.SetInteger(AttackTypeHash, (int)BossAttackType.Hide);
        SetCoroutineFlag(true);

        StartCoroutine(Attack2Instantiate(instantiateValue));
    }
    public void Attack3(int attackType)//移動して近接攻撃
    {
        SetCoroutineFlag(true);
        StartCoroutine(Attack3Move(attackType));
    }
    private float GetCameraHalfWidth() => c_Camera.orthographicSize * c_Camera.aspect;//画面の横幅の半分
    private bool IsAttackCanceled()//コルーチンを途中でやめるか(フラグが消された・ボスが倒された)
        => !b_IsBossCoroutine || c_BossBaseManager.GetDieFlag();

    IEnumerator Attack2Instantiate(int remaining)
    {
        float currentTime = Attack2Interval;//最初の1発はすぐ撃つ
        float width = GetCameraHalfWidth();
        float direction = c_BossBaseManager.CurrentDirection;

        while (true)
        {
            if (IsAttackCanceled()) break;

            c_BossBaseManager.SetIsHitFlag(true);//ジャンプ中はヒット中扱いにして攻撃を受けないようにする
            currentTime += Time.deltaTime;
            if (currentTime >= Attack2Interval)
            {
                if (remaining <= 0) break;
                currentTime = 0f;

                float posY = TatuGameManager.Instance.m_StageScaleMinY / Attack2HeightRate;
                var obj = c_ObjectPool.GetObject(CharaState.Boss, ObjctPool.EfectType.Fire);
                SetBulletInfo(obj, new Vector3(c_Camera.transform.position.x + (width * direction), posY, posY));

                remaining--;
            }
            yield return null;
        }
        SetAlpha(r_SpriteRen, 1);

        c_BossBaseManager.SetIsHitFlag(false);
        ResetAttackFlag();
    }
    IEnumerator Attack3Move(int attackType)
    {
        a_Animator.SetInteger(AttackTypeHash, (int)BossAttackType.Hide);

        float alpha = 1;
        while (true)
        {
            if (IsAttackCanceled())
            {
                SetFadeAlpha(1);
                yield break;
            }

            alpha = Mathf.Clamp01(alpha - Time.deltaTime);
            SetFadeAlpha(alpha);

            if (alpha < 0.01f)//ほぼ透明になったらプレイヤーの近くへ瞬間移動
            {
                Vector3 pl = c_PlayerMove.transform.position;
                //画面外になってもFixedUpdateのCheckGroundで画面内に収まる
                transform.position = new Vector3(pl.x + (c_BossBaseManager.CurrentDirection * Attack3Offset), pl.y, pl.z);

                SetFadeAlpha(1f);//すぐ表示
                break;
            }

            yield return null;
        }
        a_Animator.SetInteger(AttackTypeHash, attackType);

        SetCoroutineFlag(false);
    }
    private void SetFadeAlpha(float alpha)//本体と影の透明度
    {
        SetAlpha(r_SpriteRen, alpha);
        SetAlpha(r_Shadow, alpha);
    }
    private void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}
