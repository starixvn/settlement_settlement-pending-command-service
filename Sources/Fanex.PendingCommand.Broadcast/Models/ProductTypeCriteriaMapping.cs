namespace Fanex.PendingCommand.Broadcast.Models;

public class ProductTypeCriteriaMapping
{
    private readonly string _productType;
    public Type CriteriaType { get; }

    public ProductTypeCriteriaMapping(string productType, Type criteriaType)
    {
        _productType = productType;
        CriteriaType = criteriaType;
    }

    public bool Matches(string productType) => _productType == productType;
} 