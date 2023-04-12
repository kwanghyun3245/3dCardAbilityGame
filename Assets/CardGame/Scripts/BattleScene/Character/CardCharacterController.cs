using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AnimController), typeof(Health))]
[RequireComponent(typeof(CardCharacterMovement))]
public class CardCharacterController : MonoBehaviour,ICardTargetable
{
    [Header("Movement")]
    [SerializeField] CardCharacterMovement movement;

    [Header("WorldUI")]
    [SerializeField] Transform canvasTransform;
    [SerializeField] NameController charactername;

    [Header("Battle")]
    [SerializeField] Combat combat;
    [SerializeField] Collider weaponCollider;
    [SerializeField] Health health;
    [SerializeField] ETeamNum teamNum = ETeamNum.Enemy;

    [Header("Animation")]
    [SerializeField] AnimController animController;

    [Header("Ability")]
    [SerializeField] Ability_DropSlot abilityDropSlot;
    List<Ability_Origin> abilityList = new List<Ability_Origin>();

    List<CardCharacterController> enemyList;
    List<CardCharacterController> teamList;
    int spawnID;

    //테스트용 SerializeField 선언 차후 지우기
    [SerializeField] int baseDamage = 10;
    int buffDamage = 0;
    bool isAlive = true;

    public delegate void EnemyDeadDel(int spawnID, ETeamNum teamNum);
    public event EnemyDeadDel DeadEvent;
    public delegate void ActionEndDel();
    public event ActionEndDel ActionEndEvent;

    private void Awake()
    {
        if (health == null) health = this.GetComponent<Health>();
        if (animController == null) animController = this.GetComponent<AnimController>();
        if (movement == null) movement = this.GetComponent<CardCharacterMovement>();
        if (combat == null) combat = weaponCollider.GetComponent<Combat>();

        weaponCollider.enabled = false;

        health.DeadEvent += OnDead;
        health.HitEvent += OnHit;

        movement.Setting(canvasTransform);
        movement.ArriveEvent += OnArrive;

        abilityDropSlot.Setting(this);
    }

    public void Setting(int spawnID, ETeamNum teamNum, int hp,string characterName,int damage)
    {
        this.spawnID = spawnID;
        this.teamNum = teamNum;

        //hp 셋팅
        health.SetTeamNum(teamNum);
        health.SetHP(hp);

        //weapon 셋팅
        combat.Setting(teamNum,damage);

        //카드 드롭 슬롯 활성화
        abilityDropSlot.gameObject.SetActive(true);

        //이름 셋팅
        charactername.SetName(characterName);

        isAlive = true;
    }

    #region 이벤트 리스너 함수
    public void OnHit()
    {
        animController.Hit();
    }

    /// <summary>
    /// <para> 캐릭터 사망 처리 및 애니메이션 재생 </para>
    /// <para> HP가 0이되면 호출되는 함수 </para>
    /// </summary>
    public void OnDead()
    {
        isAlive = false;
        animController.Dead();
        DeadEvent?.Invoke(spawnID,teamNum);
    }

    void OnArrive()
    {
        animController.StopMove();
        Attack();
    }

    public void OnGameState(EGameState gameState)
    {
        switch (gameState)
        {
            case EGameState.게임중:
                break;
            case EGameState.게임종료:
                health.SetDeadable(false);
                break;
            case EGameState.턴시작:
                break;
            case EGameState.턴종료:
                movement.MoveBasePosition();
                buffDamage = 0; //@todo 연속된 턴 버프도 생각해보기
                break;
        }
    }

    /// <summary>
    /// <para>살아있는 아군 및 적군 객체 리스트를 전달 받는 함수</para>
    /// </summary>
    /// <param name="teamNum"></param>
    /// <param name="changeList"></param>
    public void OnTargetsChange(ETeamNum teamNum ,List<CardCharacterController> changeList)
    {
        if (this.teamNum == teamNum) teamList = changeList;
        else enemyList = changeList;
    }
    #endregion

    #region 캐릭터 행동 시작 및 종료 함수

    /// <summary>
    /// <para> 1. 캐릭터에 등록된 모든 어빌리티 발동</para>
    /// <para> 2.캐릭터 활동 시작</para>
    /// </summary>
    public void ActionStart()
    {
        if(abilityList?.Count > 0)
        {
            foreach (var ability in abilityList)
                ability.Active(this);
        }

        if(enemyList.Count > 0)
        {
            int targetNum = Random.Range(0, enemyList.Count);
            animController.StartMove();
            movement.MoveStart(enemyList[targetNum].transform.position);
        }
    }

    /// <summary>
    /// <para>ActionEndEvent를 Invoke하는 함수</para>
    /// <para>이벤트에 바인딩된 함수가 없으면 에러 로그를 띄운다.</para>
    /// </summary>
    void ActionEnd()
    {
        if (ActionEndEvent != null)
            ActionEndEvent.Invoke();
        else
            CustomDebugger.Debug(LogType.LogError, "ActionEndEvent에 바인딩된 함수가 없습니다.");
    }
    #endregion

    /// <summary>
    /// <para>공격 함수</para>
    /// </summary>
    void Attack()
    {
        //데미지 할당
        combat.DamageSet(baseDamage+buffDamage);

        //공격 애니메이션 실행 및 콜백 함수 전달
        animController.Attack(
            ()=> weaponCollider.enabled = true,
            ()=> 
            {
                weaponCollider.enabled = false;
                StartCoroutine(ActionEndWait());
            });
    }

    /// <summary>
    /// 캐릭터가 살아있는지 파악
    /// </summary>
    /// <returns>살아 있으면 true </returns>
    public bool GetIsAlive()
    {
        return isAlive;
    }

    /// <summary>
    /// <para>행동 완료 후 대기 시간</para>
    /// <para>ActionEnd함수를 호출한다.</para>
    /// </summary>
    /// <returns></returns>
    IEnumerator ActionEndWait()
    {
        yield return new WaitForSeconds(1f);
        ActionEnd();
    }

    void EventUnBind()
    {
        health.DeadEvent -= OnDead;
        health.HitEvent -= OnHit;
        movement.ArriveEvent -= OnArrive;
    }

    private void OnDestroy()
    {
        abilityList.Clear();
        EventUnBind();
        ActionEndEvent = null;
        DeadEvent = null;
    }

    #region ICardTargetable 인터페이스 구현
    public void DamageControl(int value)
    {
        buffDamage += value;
    }

    public void HPControl(int value)
    {
        if (value >= 0)
            health.Heal(value);
        else
            health.Hit(Mathf.Abs(value));
    }

    public void AbilityEquip(Ability_Origin ability)
    {
        abilityList.Add(ability);
    }

    public ETeamNum GetTeamNum()
    {
        return teamNum;
    }
    #endregion
}
