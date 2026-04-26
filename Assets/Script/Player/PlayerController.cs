using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Character {
    [Header("玩家运动属性")]
    public float jumpForce;
    public float attackRange;
    [HideInInspector] public float velocity;
    [HideInInspector] public float attackSpeed;
    private int goldInt;
    [Header("场景组件")]
    public Camera mainCamera;
    public SpriteRenderer weaponRender;
    [Header("玩家组件")]
    private Rigidbody2D rb2d;
    [SerializeField]
    public RPGattribute rpg_Character;
    public RPGattribute rpg_Amor;
    public RPGattribute rpg_Sum;
    private SpriteRenderer SpRenderer;
    private PhysicsCheck pC;
    private Animator anim;
    public Slider hpBar;
    public Text GoldText;
    public PhysicsMaterial2D Nom;
    public PhysicsMaterial2D Wall;


    [Header("变量")]
    public GameObject bullet;
    public GameObject shootPos;
    public GameObject weaponPos;
    public GameObject bulletParent;
    public PlayerItemScriptableObject playerItem;
    public float attackFreTime_T;
    public bool isAttack;
    public float hurtForce = 0.2f;
    bool weaponAttacked = false;



    // Start is called before the first frame update
    private void OnEnable() {
        ForceMouse();
        ChangeInvulnerable();
        ChangerPhyMaterial();
        RefrestHp();
        CloseAttack();
        PlayerMove();
        RPGToDo();

        rpg_Sum = RPG_sum() + rpg_Character;
        maxHp = rpg_Sum.maxHp;
        weaponRender.sprite = playerItem.weapon.weaponModel;
    }
    void Start() {
        rb2d = this.GetComponent<Rigidbody2D>();
        pC = this.GetComponent<PhysicsCheck>();
        anim = this.GetComponent<Animator>();
        Hp = maxHp;

        LoadGold();           // 加载金币
        RefrestGold();        // 更新 UI
    }

    // Update is called once per frame
    void Update() {
        ForceMouse();
        ChangeInvulnerable();
        ChangerPhyMaterial();
        RefrestHp();
        CloseAttack();
        PlayerMove();
        RPGToDo();
        // Debug.Log(RPG_sum());//
        rpg_Amor = RPG_sum();
        rpg_Sum = RPG_sum() + rpg_Character;
        maxHp = rpg_Sum.maxHp;
        weaponRender.sprite = playerItem.weapon.weaponModel;
    }
    private void FixedUpdate() {
        TurnAround();

    }
    void PlayerMove() {
        #region 移动
        float horizontal = Input.GetAxis("Horizontal"); //A D 左右


        rb2d.velocity = new Vector2(horizontal * velocity, rb2d.velocity.y);//给予速度
        #endregion
        #region 跳跃
        if (Input.GetKeyDown(KeyCode.Space) && pC.isGround) {
            anim.SetTrigger("IsJump");
            rb2d.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
        #endregion
    }
    #region 攻击
    float attackFreTime_C;
    bool inFire;
    void Attack() {

        if (Input.GetKeyUp(KeyCode.Mouse0) && !inFire) {
            anim.SetTrigger("IsShoot");
            if (this.transform.localScale.x > 0) {
                GameObject t = Instantiate(bullet, shootPos.transform.position, shootPos.transform.rotation);
                t.transform.parent = bulletParent.transform;
            }
            if (this.transform.localScale.x < 0) {
                Quaternion temp;
                temp = Quaternion.Euler(shootPos.transform.rotation.eulerAngles - new Vector3(0, 0, 180));
                GameObject t = Instantiate(bullet, shootPos.transform.position, temp);
                t.transform.parent = bulletParent.transform;
            }
            inFire = true;
        }
        if (attackFreTime_C > 0) {
            attackFreTime_C -= Time.deltaTime;
        } else {
            inFire = false;
            attackFreTime_C = attackFreTime_T;
        }

    }
    float c_AtkTime = 0;
    void CloseAttack() {

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            if (!isAttack) {
                if (weaponAttacked) {
                    weaponPos.transform.DOLocalRotate(new Vector3(0, 0, 0), attackSpeed);
                    weaponAttacked = false;
                    isAttack = true;
                    c_AtkTime = rpg_Sum.atkSpeed;
                    return;
                } else {
                    weaponPos.transform.DOLocalRotate(new Vector3(0, 0, attackRange), attackSpeed);
                    weaponAttacked = true;
                    isAttack = true;
                    c_AtkTime = rpg_Sum.atkSpeed;
                    return;
                }
            }
        }

        if (c_AtkTime > 0) {
            c_AtkTime -= Time.deltaTime;
        } else if (c_AtkTime <= 0) {
            c_AtkTime = 0;
            isAttack = false;
        }

    }
    void TurnAround() {
        #region 切换面向
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        int faceDir = (int)transform.localScale.x;//切换朝向
        if (mouseWorldPosition.x > this.transform.position.x) {
            faceDir = 1;

        } else if (mouseWorldPosition.x < this.transform.position.x) {
            faceDir = -1;
        }
        transform.localScale = new Vector3(faceDir, 1, 1);

        #endregion
    }
    void ForceMouse() {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        // 使物体朝向鼠标位置
        Vector3 direction = mouseWorldPosition - transform.position;

        // 获取物体当前的旋转
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (this.transform.localScale.x < 0) // 如果角色翻转，调整角度
        {
            angle += 180f;
        }
        // 设置物体的旋转（绕 Z 轴旋转）
        shootPos.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    #endregion

    #region 基础方法
    public void GetHurt(Transform attack) {
        isHurt = true;
        rb2d.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attack.position.x), 0).normalized;
        rb2d.AddForce(dir * hurtForce, ForceMode2D.Impulse);
    }
    public void RefrestHp() {
        hpBar.value = Hp / maxHp;
    }
    public void RefrestGold() {
        GoldText.text = ($"{goldInt}");
    }
    public void ChangeGold(int a) {
        goldInt = goldInt + a;
        RefrestGold();
        SaveGold();
    }
    private void SaveGold() {
        PlayerPrefs.SetInt("PlayerGold", goldInt);
    }
    private void LoadGold() {
        goldInt = PlayerPrefs.GetInt("PlayerGold", 0);
    }
    void ChangerPhyMaterial() {
        rb2d.sharedMaterial = pC.isGround ? Nom : Wall;
    }
    void ChangeInvulnerable() {
        if (invulnerable) {
            invulnerableCounter -= Time.deltaTime;
            if (invulnerableCounter < 0) {
                invulnerable = false;
            }
        }
    }

    public void Heal(float amount) {
        Hp += amount;
        if (Hp > maxHp)
            Hp = maxHp;
        RefrestHp();          // 更新血条 UI
    }
    #endregion
    #region RPG结算
    public void clearItem() {
        playerItem.weapon = null;
        playerItem.armorHead = null;
        playerItem.armorBody = null;
        playerItem.armorHand = null;
        playerItem.armorFoot = null;
        rpg_Sum = rpg_Character;
    }
    public void clearGold() {
        goldInt = 0;
        SaveGold();
        RefrestGold();
    }

    void RPGToDo() {
        this.velocity = rpg_Sum.moveSpeed;
        maxHp = rpg_Sum.maxHp;
    }
    RPGattribute RPG_sum() {
        RPGattribute weaponRPG = new RPGattribute();
        if (playerItem.weapon != null) {
            weaponRPG = weaponEndAttribute(playerItem.weapon);
        }

        RPGattribute rpg_A = playerItem.armorHand.attriute +
             playerItem.armorBody.attriute +
             playerItem.armorHand.attriute +
             playerItem.armorFoot.attriute +
             weaponRPG +
             playerItem.weapon.weaponsEntry_a.weapWords +
             playerItem.weapon.weaponsEntry_b.weapWords +
             playerItem.weapon.weaponsEntry_c.weapWords;
        return rpg_A;
    }
    public RPGattribute weaponEndAttribute(WeaponClass weapon) {
        RPGattribute weaponEndAttribute = new RPGattribute();

        // 设置基础攻击力（直接从武器的伤害值获取）
        weaponEndAttribute.atk = weapon.damage;

        // 处理武器条目 a, b, c
        UpdateWeaponAttributes(ref weaponEndAttribute, weapon.weaponsEntry_a);
        UpdateWeaponAttributes(ref weaponEndAttribute, weapon.weaponsEntry_b);
        UpdateWeaponAttributes(ref weaponEndAttribute, weapon.weaponsEntry_c);

        // 处理独特条目
        ApplyUniqueEntry(ref weaponEndAttribute, weapon.weaponsEntry_a);

        return weaponEndAttribute;
    }

    // 更新武器条目的属性
    private void UpdateWeaponAttributes(ref RPGattribute weaponEndAttribute, WeaponsEntryClass entry) {
        if (entry == null || entry.valueType == null)
            return;

        for (int i = 0; i < entry.valueType.Length; i++) {
            ramTypeClass entryValue = entry.valueType[i];
            ApplyRamValueType(ref weaponEndAttribute, entryValue);
        }
    }

    // 处理 ramValueType 类型的属性更新
    private void ApplyRamValueType(ref RPGattribute weaponEndAttribute, ramTypeClass entryValue) {
        switch (entryValue.ramValueType) {
            case ramValueType.MaxHp:
            weaponEndAttribute.maxHp += entryValue.realValue;
            break;
            case ramValueType.MaxMp:
            weaponEndAttribute.maxMp += entryValue.realValue;
            break;
            case ramValueType.Atk:
            weaponEndAttribute.atk += entryValue.realValue;
            break;
            case ramValueType.AtkSpeed:
            weaponEndAttribute.atkSpeed += entryValue.realValue;
            break;
            case ramValueType.CritChange:
            weaponEndAttribute.crit_Change += entryValue.realValue;
            break;
            case ramValueType.CritDamage:
            weaponEndAttribute.crit_Damage += entryValue.realValue;
            break;
            case ramValueType.Def:
            weaponEndAttribute.def += entryValue.realValue;
            break;
            case ramValueType.Luck:
            weaponEndAttribute.luck += entryValue.realValue;
            break;
            case ramValueType.MoveSpeed:
            weaponEndAttribute.moveSpeed += entryValue.realValue;
            break;
            case ramValueType.VampireAtk:
            weaponEndAttribute.vampire_Atk += entryValue.realValue;
            break;
            case ramValueType.RecoveryHp:
            weaponEndAttribute.recovery_HP += entryValue.realValue;
            break;
            case ramValueType.RecoveryMp:
            weaponEndAttribute.recovery_Mp += entryValue.realValue;
            break;
        }
    }

    // 处理独特条目的应用
    private void ApplyUniqueEntry(ref RPGattribute weaponEndAttribute, WeaponsEntryClass entry) {
        if (entry == null || entry.uniqueEntry == uniqueEntry.Null)
            return;

        switch (entry.uniqueEntry) {
            case uniqueEntry.mustCrit:
            weaponEndAttribute.crit_Change = 1f; // 强制暴击
            break;
            case uniqueEntry.poison:
            // 可以在此添加中毒效果或其他独特效果
            break;
        }
    }

    #endregion
}
