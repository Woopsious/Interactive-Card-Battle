using TMPro;
using UnityEngine;

public class Entity : MonoBehaviour, IDamagable
{
	public EntityData entityData;

	public TMP_Text entityNameText;
	public TMP_Text entityHealthText;

	int health;

	void Start()
	{
		SetupEntity();
	}

	void SetupEntity()
	{
		if (entityData == null)
		{
			Debug.LogError("Entity data null");
			return;
		}

		string cardName = entityData.entityName + Random.Range(1000, 9999);
		gameObject.name = cardName;
		entityNameText.text = cardName;
		health = entityData.maxHealth;
		UpdateHealthUi();
	}

	public void RecieveDamage(int damage)
	{
		health -= damage;
		UpdateHealthUi();

		OnDeath();
	}

	void UpdateHealthUi()
	{
		entityHealthText.text = "HEALTH\n" + health + "/" + entityData.maxHealth;
	}

	void OnDeath()
	{
		if (health >= 0)
		{
			//entity died
		}
	}
}
