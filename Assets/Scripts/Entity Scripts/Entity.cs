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

	public int health;
	public int block;

	public List<EnemyMoveSet> enemyMovesList = new();
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
		if (entityData.moveSetOrder.Count == 0)
		{
			Debug.LogError("Entity move set order at 0");
			return;
		}

		previousUsedMove = null;
		previousUsedMoveIndex = -1;

		enemyMovesList.Clear();

		foreach(MoveSetData moveSet in entityData.moveSetOrder)
			enemyMovesList.Add(new EnemyMoveSet(this, moveSet));
	}

	//start new round event
	void NewRoundStart()
	{
		foreach (EnemyMoveSet move in enemyMovesList)
			move.NewRound();
	}

	//start/end turn events
	protected virtual void StartTurn(Entity entity)
	{
		if (entity != this) return; //not this entities turn

		block = 0;
		UpdateUi();

		if (entityData.isPlayer) return; //if is player shouldnt need to do anything else as other scripts handle it

		PickNextMove();
	}
	public virtual void EndTurn()
	{
		OnTurnEndEvent?.Invoke(this);
	}

	//new enemy entity attacks
	async void PickNextMove()
	{
		int nextMoveIndex = previousUsedMoveIndex + 1;

		Debug.LogError("next attack index: " + nextMoveIndex + " | attacks count: " + enemyMovesList.Count);

		if (nextMoveIndex >= enemyMovesList.Count) //end of attack queue, loop back
		{
			Debug.LogError("end of attack queue");

			previousUsedMoveIndex = -1;
			PickNextMove();
			return;
		}


		if (enemyMovesList[nextMoveIndex].CanUseMoveFromMoveSet())
		{
			Debug.LogError("using attack");

			await UseMove(nextMoveIndex);
			return;
		}
		else if (HasMoveAvailable()) //try next move set
		{
			Debug.LogError("has attack trying next");

			previousUsedMoveIndex++;
			PickNextMove();
			return;
		}
		else //end turn if no moves available in move sets to avoid endless loop
		{
			Debug.LogError("end turn");

			EndTurn();
			return;
		}
	}
	bool HasMoveAvailable()
	{
		foreach (EnemyMoveSet moveSet in enemyMovesList)
		{
			if (moveSet.CanUseMoveFromMoveSet())
				return true;
		}

		return false;
	}
	async Task UseMove(int nextMoveIndex)
	{
		AttackData attackData = enemyMovesList[nextMoveIndex].UseMove().attackData;

		previousUsedMoveIndex = nextMoveIndex;
		previousUsedMove = attackData;
		OnEnemyMoveFound?.Invoke(attackData);

		await Task.Delay(3000);

		CardUi card = GameManager.instance.SpawnCard();
		card.SetupCard(attackData);
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
