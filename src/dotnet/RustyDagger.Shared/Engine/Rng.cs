namespace RustyDagger.Shared.Engine;

/// <summary>
/// Faithful port of the Dragon Court RNG system.
/// Uses .NET Random internally but exposes the same API as the original Java code.
/// </summary>
public class Rng
{
    private readonly Random _random;

    public Rng() => _random = new Random();
    public Rng(int seed) => _random = new Random(seed);

    /// <summary>Roll 0 to value-1</summary>
    public int Roll(int value) => value <= 0 ? 0 : _random.Next(value);

    /// <summary>Roll 0 to value, bell-curve centered on value/2. Equivalent to roll(value/2+1) + roll((value+1)/2+1) - 1</summary>
    public int Twice(int value)
    {
        if (value <= 0) return 0;
        int half1 = value / 2 + 1;
        int half2 = (value + 1) / 2 + 1;
        return _random.Next(half1) + _random.Next(half2);
    }

    /// <summary>Contest: returns true if a wins against b. roll(a+b) &lt; a</summary>
    public bool Contest(int a, int b) => Roll(a + b) < a;

    /// <summary>Percent chance (0-100). Returns true if roll(100) &lt; chance.</summary>
    public bool Percent(int chance) => Roll(100) < chance;

    /// <summary>Chance check: roll(value) == 0, i.e., 1-in-value chance.</summary>
    public bool Chance(int value) => value > 0 && Roll(value) == 0;

    /// <summary>Spread a value with bell-curve randomization.
    /// min = 5*value/7, result = 1 + min + twice(value - min)</summary>
    public int Spread(int value)
    {
        if (value <= 0) return 1;
        int min = 5 * value / 7;
        return 1 + min + Twice(value - min);
    }

    /// <summary>Skew: returns a random value weighted toward low end. roll(roll(value)+1)</summary>
    public int Skew(int value)
    {
        if (value <= 0) return 0;
        return Roll(Roll(value) + 1);
    }

    /// <summary>Pick a random element from an array.</summary>
    public T Pick<T>(T[] array) => array[Roll(array.Length)];

    /// <summary>Pick a random element from a list.</summary>
    public T Pick<T>(IList<T> list) => list[Roll(list.Count)];
}
