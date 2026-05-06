namespace Beetles.Domain.Entities;

public class Wall : BitemporalEntity
{
    public required string Color { get; set; }

    public override Wall Clone()
    {
        var wall = (Wall)base.Clone();

        return wall;
    }
}
