using System;

public class Building
{
    public string Name { get; private set; }
    private IDurability durability;

    public Building(string name, int maxDurability)
    {
        Name = name;
        durability = new Durability(maxDurability);
    }

    public void Use()
    {
        durability.Decrease(5); // 예시로 사용할 때마다 내구도 5 감소
        if (durability.IsBroken)
        {
            Console.WriteLine($"{Name} is broken!");
        }
        else
        {
            Console.WriteLine($"{Name} durability: {durability.CurrentDurability}/{durability.MaxDurability}");
        }
    }

    public void Repair(int amount)
    {
        durability.Repair(amount);
        Console.WriteLine($"{Name} repaired. Durability: {durability.CurrentDurability}/{durability.MaxDurability}");
    }

    public float GetDurabilityPercentage()
    {
        return durability.DurabilityPercentage;
    }
}
