using System;
using TMPro;
using UnityEngine;
using static CardData;
using static UnityEngine.Rendering.GPUSort;

public class Entity : MonoBehaviour, IDamagable
{
	public EntityData entityData;

	public TMP_Text entityNameText;
	public TMP_Text entityHealthText;
	public TMP_Text entityblockText;

	int health;
	public int block;

	public static event Action<CardData> OnCardChosen;

	public static event Action<Entity> OnTurnEndEvent;

	void Start()
	{
		SetupEntity();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += StartTurn;
		ShowPlayedCardUi.OnThrowCardAfterShown += ThrowCardAtPlayer;
	}
	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= StartTurn;
		ShowPlayedCardUi.OnThrowCardAfterShown -= ThrowCardAtPlayer;
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
	}

	//start/end turn events
	void StartTurn(Entity entity)
	{
		if (entity != this) return; //not this entities turn

		block = 0;
		UpdateUi();

		if (entityData.isPlayer) return; //if is player shouldnt need to do anything else as other scripts handle it

		//if is non player atm choose random card to use + show player the card enemy will use
		//wait a few seconds then throw card at player, wait till card hits player/gets blocked then end turn


		PickCardToPlay();
		//ThrowCardAtPlayer(GameManager.instance.GetDamageCard(entityData.cards));
	}
	public void EndTurn()
	{
		OnTurnEndEvent?.Invoke(this);
	}

	//enemy entity attacks
	void PickCardToPlay()
	{
		CardData chosenCard = GameManager.instance.GetDamageCard(entityData.cards);
		OnCardChosen?.Invoke(chosenCard);
	}

	void ThrowCardAtPlayer(CardData cardData)
	{
		if (TurnOrderManager.Instance.currentEntityTurn != this) return;

		CardUi card = GameManager.instance.SpawnCard();
		card.SetupCard(cardData, false);
		card.GetComponent<ThrowableCard>().EnemyThrowCard(this, TurnOrderManager.Instance.playerEntity.transform.localPosition);
	}

	//entity hits via cards
	public void OnHit(DamageData damageData)
	{
		if (damageData.DamageType == DamageType.block)
			AddBlock(damageData.Damage);
		else if (damageData.DamageType == DamageType.heal)
			RecieveHealing(damageData.Damage);
		else if (damageData.DamageType == DamageType.physical)
			RecieveDamage(damageData.Damage);
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
