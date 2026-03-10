using System;

public enum RelationshipType
{
    Acquaintance,
    Couple,
    Colleague,
    Conflict
}

[Serializable]
public class Relationship
{
    public CitizenAI Other;
    public RelationshipType Type;
}
