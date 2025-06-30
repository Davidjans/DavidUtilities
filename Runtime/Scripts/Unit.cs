using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName = "New Unit";
    public Faction faction;
    [Range(10, 500)] public int health = 100;
    public float moveSpeed = 5.0f;
    
    [Header("Combat Stats")]
    public int attackDamage = 10;
    public float attackRange = 1.5f;
    public AttackType attackType = AttackType.Melee;

    public enum Faction
    {
        Player,
        Enemy,
        Neutral
    }
    
    public enum AttackType
    {
        Melee,
        Ranged,
        Magic
    }
}