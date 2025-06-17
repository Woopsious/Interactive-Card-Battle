using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static DamageData;

public class Entity : MonoBehaviour, IDamagable
{
	public EntityData entityData;

	public TMP_Text entityNameText;
	public TMP_Text entityHealthText;
	public TMP_Text entityblockText;

	int health;
	public int block;

	public List<EnemyMove> enemyMovesList = new();
	public AttackData previousUsedMove;
	public int previousUsedMoveIndex;

	public static event Action<AttackData> OnEnemyMoveFound;
	public static event Action OnEnemyAttack;

	public static event Action<Entity> OnTurnEndEvent;

	void Start()
	{
		SetupEntity();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewRoundStartEvent += NewRoundStart;
		TurnOrderManager.OnNewTurnEvent += StartTurn;
	}
	void OnDisable()
	{
		TurnOrderManager.OnNewRoundStartEvent -= NewRoundStart;
		TurnOrderManager.OnNewTurnEvent -= StartTurn;
	}

	void SetupEntity()
	{
		if (entityData == null)
		{
			Debug.LogError("Entity data null");
			return;
		}

		string cardName = entityData.entityName + UnityEngine.Random.Range(1000, 9999);
		gameObject.name = cardName;
		entityNameText.text = cardName;
		health = entityData.maxHealth;
		block = 0;
		UpdateUi();

		if (entityData.isPlayer) return;

		SetupEnemyAttacks();
	}
	void SetupEnemyAttacks()
	{
		if (entityData.attacks.Count == 0)
		{
			Debug.LogError("Entity attack data count 0");
			return;
		}

		previousUsedMove = null;
		previousUsedMoveIndex = -1;
		enemyMovesList.Clear();

		foreach(AttackData attackData in entityData.attacks)
			enemyMovesList.Add(new EnemyMove(attackData));
	}

	//start new round event
	void NewRoundStart()
	{
		foreach (EnemyMove attack in enemyMovesList)
			attack.NewRound();
	}

	//start/end turn events
	protected virtual void StartTurn(Entity entity)
	{
		if (entity != this) return; //not this entities turn

		block = 0;
		UpdateUi();

		if (entityData.isPlayer) return; //if is player shouldnt need to do anything else as other scripts handle it

		PickNextAttack();
	}
	public virtual void EndTurn()
	{
		OnTurnEndEvent?.Invoke(this);
	}

	//new enemy entity attacks
	async void PickNextAttack()
	{
		int nextMoveIndex = previousUsedMoveIndex + 1;

		Debug.LogError("next attack index: " + nextMoveIndex + " | attacks count: " + enemyMovesList.Count);

		if (nextMoveIndex >= enemyMovesList.Count) //end of attack queue, loop back
		{
			Debug.LogError("end of attack queue");

			previousUsedMoveIndex = -1;
			PickNextAttack();
			return;
		}


		if (enemyMovesList[nextMoveIndex].CanUseAttack())
		{
			Debug.LogError("using attack");

			await UseAttack(nextMoveIndex);
			return;
		}
		else if (HasAnAttackAvailable()) //try next attack
		{
			Debug.LogError("has attack trying next");

			previousUsedMoveIndex++;
			PickNextAttack();
			return;
		}
		else //end turn if no next attack available to avoid endless loop
		{
			Debug.LogError("end turn");

			EndTurn();
			return;
		}
	}
	bool HasAnAttackAvailable()
	{
		foreach (EnemyMove move in enemyMovesList)
		{
			if (move.CanUseAttack())
				return true;
		}

		return false;
	}
	async Task UseAttack(int nextMoveIndex)
	{
		AttackData moveData = enemyMovesList[nextMoveIndex].attackData;

		previousUsedMoveIndex = nextMoveIndex;
		previousUsedMove = moveData;

		enemyMovesList[previousUsedMoveIndex].UseAttack();
		OnEnemyMoveFound?.Invoke(moveData);

		await Task.Delay(3000);

		CardUi card = GameManager.instance.SpawnCard();
		card.SetupCard(moveData);
		card.GetComponent<ThrowableCard>().EnemyThrowCard(this, TurnOrderManager.Instance.playerEntity.transform.localPosition);

		OnEnemyAttack?.Invoke();
	}

	//entity hits via cards
	public void OnHit(DamageData damageData)
	{
		if (damageData.damageType == DamageType.block)
			AddBlock(damageData.damage);
		else if (damageData.damageType == DamageType.heal)
			RecieveHealing(damageData.damage);
		else if (damageData.damageType == DamageType.physical)
			RecieveDamage(damageData.damage);
		else
			Debug.LogError("no hit type set up");
	}
	void AddBlock(int block)
	{
		this.block += block;
		UpdateUi();
	}
	void RecieveHealing(int healing)
	{
		health += healing;
		if (health > entityData.maxHealth)
			health = entityData.maxHealth;

		UpdateUi();
	}
	void RecieveDamage(int damage)
	{
		damage = GetBlockedDamage(damage);
		health -= damage;
		UpdateUi();
		OnDeath();
	}
	int GetBlockedDamage(int damage)
	{
		if (block > damage)
		{
			block -= damage;
			damage = 0;
		}
		else
		{
			damage -= block;
			block = 0;
		}

		return damage;
	}

	void UpdateUi()
	{
		entityHealthText.text = "HEALTH\n" + health + "/" + entityData.maxHealth;
		entityblockText.text = "Block\n" + block;
	}

	void OnDeath()
	{
		if (health >= 0)
		{
			//entity died
		}
	}
}
