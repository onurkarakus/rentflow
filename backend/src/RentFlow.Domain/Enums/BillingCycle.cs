namespace RentFlow.Domain.Enums;

/// <summary>
/// Represents the billing cycle for subscriptions.
/// </summary>
public enum BillingCycle
{
    /// <summary>
    /// Monthly billing cycle.
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Annual billing cycle (usually with discount).
    /// </summary>
    Annual = 2
}