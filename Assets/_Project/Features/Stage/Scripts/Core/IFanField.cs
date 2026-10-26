using UnityEngine;

public interface IFanField
{
    Vector2 WindVector { get; }      // •—‚ÌŒü‚«
    float UpdraftMultiplier { get; } //ã¸•ûŒü‚Ì•â³
    int Priority { get; }
}
