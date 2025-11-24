using RentFlow.Domain.Common;
using RentFlow.Domain.Enums;

namespace RentFlow.Domain.Entities.Subscriptions;

/// <summary>
/// Represents the subscription history of a tenant.
/// Tracks all subscription changes, payments, and vehicle count changes.
/// </summary>
public class SubscriptionHistory : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant ID this subscription history belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the subscription plan entity ID.
    /// </summary>
    public Guid SubscriptionPlanId { get; set; }

    /// <summary>
    /// Gets or sets the start date of this subscription period.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of this subscription period.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether this is the currently active subscription.
    /// Only one subscription per tenant should be active at a time.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the vehicle count at the start of this subscription period.
    /// Used to calculate pricing and track growth.
    /// </summary>
    public int VehicleCountAtStart { get; set; }

    /// <summary>
    /// Gets or sets the vehicle count at the end of this subscription period.
    /// Null if period is still active.
    /// </summary>
    public int? VehicleCountAtEnd { get; set; }

    /// <summary>
    /// Gets or sets the billing cycle (Monthly, Annual).
    /// </summary>
    public BillingCycle BillingCycle { get; set; }

    /// <summary>
    /// Gets or sets the base plan price for this period.
    /// Example: ₺1.499 for Professional plan.
    /// </summary>
    public decimal BasePlanPrice { get; set; }

    /// <summary>
    /// Gets or sets additional charges for vehicles beyond plan limit.
    /// Example: If plan includes 20 vehicles but tenant has 25, this is (5 × PerVehiclePrice).
    /// </summary>
    public decimal AdditionalVehicleCharges { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this subscription period.
    /// Total = BasePlanPrice + AdditionalVehicleCharges
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the currency code (e.g., "TRY", "USD", "EUR").
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Gets or sets the payment status.
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>
    /// Gets or sets the payment date.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the payment method used (e.g., "CreditCard", "BankTransfer").
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the transaction ID from payment provider (Iyzico).
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Gets or sets whether auto-renewal is enabled.
    /// </summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>
    /// Gets or sets the type of subscription change.
    /// </summary>
    public SubscriptionChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets the reason for subscription change (upgrade, downgrade, cancellation).
    /// </summary>
    public string? ChangeReason { get; set; }

    /// <summary>
    /// Gets or sets the user who made this subscription change.
    /// </summary>
    public string? ChangedBy { get; set; }

    /// <summary>
    /// Gets or sets additional notes about this subscription period.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the previous subscription plan ID (for upgrades/downgrades).
    /// </summary>
    public Guid? PreviousSubscriptionPlanId { get; set; }

    /// <summary>
    /// Gets or sets the cancellation date if subscription was cancelled.
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// Gets or sets the cancellation reason if subscription was cancelled.
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Gets or sets prorated credit applied from previous plan.
    /// Used when upgrading/downgrading mid-period.
    /// </summary>
    public decimal? ProratedCredit { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the tenant this subscription history belongs to.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Gets or sets the subscription plan entity.
    /// </summary>
    public SubscriptionPlanEntity SubscriptionPlan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the previous subscription plan (for reference).
    /// </summary>
    public SubscriptionPlanEntity? PreviousSubscriptionPlan { get; set; }
}