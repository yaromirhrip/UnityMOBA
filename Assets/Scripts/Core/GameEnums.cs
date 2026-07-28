namespace MobaPrototype
{
    /// <summary>
    /// The two competing sides of the match.
    /// </summary>
    public enum TeamId
    {
        Ally = 0,
        Enemy = 1
    }

    /// <summary>
    /// What kind of unit a Health component belongs to.
    /// Used by GameManager to decide how to score kills and check win conditions.
    /// </summary>
    public enum UnitType
    {
        Player = 0,
        Creep = 1,
        Tower = 2,
        Base = 3
    }
}
