namespace RentFlow.Domain.Enums;

/// <summary>
/// Represents the type of subscription change.
/// </summary>
public enum SubscriptionChangeType
{
    /// <summary>
    /// New subscription created (usually for new tenants).
    /// </summary>
    NewSubscription = 1,

    /// <summary>
    /// Renewed the same plan.
    /// </summary>
    Renewal = 2,

    /// <summary>
    /// Upgraded to a higher-tier plan.
    /// </summary>
    Upgrade = 3,

    /// <summary>
    /// Downgraded to a lower-tier plan.
    /// </summary>
    Downgrade = 4,

    /// <summary>
    /// Subscription cancelled by user.
    /// </summary>
    Cancellation = 5,

    /// <summary>
    /// Vehicle count increased within the same plan.
    /// </summary>
    VehicleCountIncrease = 6,

    /// <summary>
    /// Vehicle count decreased within the same plan.
    /// </summary>
    VehicleCountDecrease = 7
}