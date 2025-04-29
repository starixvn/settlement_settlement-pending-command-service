namespace Fanex.PendingCommand.Models;

public class SportTypeCriteriaMapping
{
    private readonly int? specificType;

    private readonly Range? range;

    public Type CriteriaType { get; }

    // Constructor for specific sport type
    public SportTypeCriteriaMapping(int sportType, Type criteriaType)
    {
        specificType = sportType;
        CriteriaType = criteriaType;
    }

    // Constructor for range
    public SportTypeCriteriaMapping(Range range, Type criteriaType)
    {
        this.range = range;
        CriteriaType = criteriaType;
    }

    public bool Matches(int sportType) =>
        specificType.HasValue
            ? specificType.Value == sportType
            : range.HasValue && sportType >= range.Value.Start.Value && sportType <= range.Value.End.Value;
}