namespace RustyDagger.Shared.Models;

public class PackItem
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; } = 1;

    public PackItem() { }
    public PackItem(string name, int count = 1)
    {
        Name = name;
        Count = count;
    }
}
