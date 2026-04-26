using System.Collections;
using UnityEngine;

public class Enemy : Character {
    [HideInInspector] public Rigidbody2D rb2d;            // Rigidbody2D 组件
    [HideInInspector] public Animator anim;               // 动画组件
    [HideInInspector] public PhysicsCheck physicsCheck;   // 物理检测工具
    [HideInInspector] public EnemyAnim enemyAnim;
    public GameObject bullet;                              // 子弹对象预制体
    public GameObject shootPos;                            // 射击位置对象
    public EnemyBaseState inPatrol;                        // 敌人巡逻状态
    public EnemyBaseState inAttack;                        // 敌人攻击状态
    public RPGattribute enemyRpgAttribute;

    [Header("索敌属性")]
    public float enemySeekRange;                           // 敌人索敌范围
    public float enemyAttackRange;                         // 敌人攻击范围
    public float enemyMoveSpeed;                           // 敌人移动速度
    public float enemyRunSpeed;                            // 敌人奔跑速度
    public LayerMask collisionLayer;                       // 碰撞检测图层

    public float enemyThinkTime;                           // 敌人思考时间
    private float enemyThinkTimeCounter;                   // 敌人思考时间计数器

    private Vector3 playerCenterPoint;                     // 玩家中心点位置

    public float enemyAttackFre;                           // 敌人攻击频率

    [Header("敌人状态")]
    [HideInInspector] public EnemyBaseState currentState;  // 敌人当前状态
    [HideInInspector] public GameObject player;            // 玩家对象

    [Header("金币")]
    public int goldDropAmount = 10;  // 死亡掉落的金币数量
    public bool hasDroppedGold = false;  // 防止重复掉落
    protected virtual void Awake() {
        rb2d = this.GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player");
        anim = this.GetComponentInChildren<Animator>();
        inPatrol = new EnemyOnSkyBySuspenInPatrol();
        inAttack = new EnemyOnSkyBySuspenInAttack();
        enemyAnim = this.GetComponent<EnemyAnim>();
        maxHp = enemyRpgAttribute.maxHp;
        enemyMoveSpeed = enemyRpgAttribute.moveSpeed / 2;
        enemyRunSpeed = enemyRpgAttribute.moveSpeed;
        enemyAttackFre = enemyRpgAttribute.atkSpeed;
        SwichState(enemyStateOnSky.enemyPatrol);

        OnDie.AddListener(ActionDie);
    }
    #region 状态机内切换
    private void OnEnable() {
        currentState.OnEnter(this);
    }
    private void Update() {
        currentState.LogicUpdate();
        if (currentState == inPatrol) { }
        if (currentState == inAttack) { }
        playerCenterPoint = new Vector3(player.transform.position.x, player.transform.position.y + player.transform.localScale.y / 2, player.transform.position.z);
        if (invulnerable) {
            invulnerableCounter -= Time.deltaTime;
            if (invulnerableCounter < 0) {
                invulnerable = false;
            }
        }
    }
    private void FixedUpdate() {
        currentState.PhysicsUpdate();
        FindPlayer();
        AutoTurn();
    }
    private void OnDisable() {
        currentState.OnExit();
    }

    public void SwichState(enemyStateOnSky t_State) {
        switch (t_State) {
            case enemyStateOnSky.enemyAttack:
            currentState = inAttack;
            break;
            case enemyStateOnSky.enemyPatrol:
            currentState = inPatrol;
            break;
            default:
            currentState = inPatrol;
            break;
        }
        currentState.OnEnter(this);
    }

    #endregion
    public void test() {
        TurnAround();
    }
    public void TurnAround() {
        float s = this.GetComponentInParent<Transform>().localScale.x;
        this.GetComponentInParent<Transform>().localScale = new Vector2(-s, this.GetComponentInParent<Transform>().localScale.y);
    }
    #region 通用方法
    public bool FindPlayer() {
        RaycastHit2D hit = Physics2D.Linecast(this.transform.position, new Vector2(player.transform.position.x, (player.transform.position.y + (player.transform.localScale.y / 2))), collisionLayer);
        Color linecolor = Color.white;
        if (hit.collider != null) {
            linecolor = Color.green;
            Debug.DrawLine(this.transform.position, new Vector2(player.transform.position.x, (player.transform.position.y + (player.transform.localScale.y / 2))), linecolor);
            return false;
        } else {
            linecolor = Color.white;
            Debug.DrawLine(this.transform.position, new Vector2(player.transform.position.x, (player.transform.position.y + (player.transform.localScale.y / 2))), linecolor);
            return true;
        }
    }
    public bool FacePlayer() {
        if ((this.gameObject.transform.position.x >= player.transform.position.x && this.gameObject.transform.localScale.x > 0) || (this.gameObject.transform.position.x <= player.transform.position.x && this.gameObject.transform.localScale.x < 0)) {
            return true;
        } else {
            return false;
        }
    }
    bool EnemyWithPlayer() {
        if (this.gameObject.transform.position.x >= player.transform.position.x) {
            return true;
        } else {
            return false;
        }
    }
    void AutoTurn() {
        if (!FacePlayer()) {
            TurnAround();
        }
    }
    public virtual void ActionDie() {
        if (!hasDroppedGold) {
            DropGold();
            hasDroppedGold = true;
        }
        anim.SetTrigger("OnDie");
        Destroy(this.gameObject, 1f);
    }


    protected void DropGold() {
        if (player != null) {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) {
                pc.ChangeGold(goldDropAmount);
                Debug.Log($"怪物死亡，掉落 {goldDropAmount} 金币");
            }
        }
    }
    #endregion
    #region 攻击方法


    public void lockPlayer() {
        if (shootPos == null) {
            return;
        }
        Vector2 v2diffent = new Vector2((player.transform.position.x - shootPos.transform.position.x), (player.transform.position.y + player.transform.localScale.y / 2 - shootPos.transform.position.y));
        float diffentRotion = Mathf.Atan(v2diffent.y / v2diffent.x) / Mathf.PI * 180;

        if (EnemyWithPlayer()) {
            shootPos.transform.eulerAngles = new Vector3(0, 0, diffentRotion + 180);
        } else {
            shootPos.transform.eulerAngles = new Vector3(0, 0, diffentRotion);
        }
    }
    private Coroutine shootCoroutine;
    public void ShootByThrid() {
        if (shootCoroutine != null) {
            StopCoroutine(shootCoroutine);
        }
        shootCoroutine = StartCoroutine("SkyShoot");
    }
    public void ShootPlayer(GameObject bullet, GameObject shootPos) {
        bullet = Instantiate(bullet, shootPos.transform.position, shootPos.transform.rotation);
        bullet.GetComponent<Bullet>().enemy = this;
    }
    public IEnumerator SkyShoot() {
        yield return new WaitForSeconds(0.1f);
        ShootPlayer(bullet, shootPos);
        yield return new WaitForSeconds(0.1f);
        ShootPlayer(bullet, shootPos);
        yield return new WaitForSeconds(0.1f);
        ShootPlayer(bullet, shootPos);
        yield return new WaitForSeconds(enemyAttackFre);

        shootCoroutine = null;
    }
    public void ClosePlayer(Transform player) {
        if (Vector2.Distance(this.transform.position, playerCenterPoint) >= enemyAttackRange) {

            Vector2 direction = new Vector2((playerCenterPoint.x - this.transform.position.x), (playerCenterPoint.y - this.transform.position.y)).normalized;
            rb2d.velocity = direction * enemyRunSpeed;
        } else {
            rb2d.velocity = Vector2.zero;
        }
    }
    #endregion
    #region 巡逻方法

    public void RamMove() {
        int r = UnityEngine.Random.Range(0, 2);
        float t = UnityEngine.Random.Range(0, 3);
        float s = this.GetComponentInParent<Transform>().localScale.x;


        if (r == 0) {
            this.GetComponentInParent<Transform>().localScale = new Vector2(-s, this.GetComponentInParent<Transform>().localScale.y);
        }

        if (r == 1) {
            MoveForTime(rb2d, new Vector2(-this.transform.localScale.x, 0), enemyMoveSpeed, t);
        }
    }

    private Coroutine moveCoroutine;
    public void MoveForTime(Rigidbody2D rb, Vector2 direction, float speed, float duration) {

        direction = direction.normalized;
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveCoroutine(rb, direction, speed, duration));
    }
    private IEnumerator MoveCoroutine(Rigidbody2D rb, Vector2 direction, float speed, float duration) {

        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            Vector2 movement = direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        moveCoroutine = null;
    }
    #endregion
}
public enum enemyStateOnSky {
    enemyAttack,
    enemyPatrol
}
