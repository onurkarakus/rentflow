namespace RentFlow.Domain.Enums;

/// <summary>
/// Represents the payment status for a subscription.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment completed successfully.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment was refunded.
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Payment was cancelled.
    /// </summary>
    Cancelled = 5
}