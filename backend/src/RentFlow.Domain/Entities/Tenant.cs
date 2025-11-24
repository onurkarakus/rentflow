using RentFlow.Domain.Common;
using RentFlow.Domain.Entities.Subscriptions;
using RentFlow.Domain.Enums;
using System;

namespace RentFlow.Domain.Entities;

public class Tenant : BaseAuditableEntity
{
    public string CompanyName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string? ContactPhone { get; set; }

    public TenantStatus Status { get; set; } = TenantStatus.Demo;

    public Guid SubsscriptionPlanId { get; set; }

    public int CurrentVehicleCount { get; set; }

    public string? Settings { get; set; }
    public string TimeZone { get; set; } = "Europe/Istanbul";
    public string Culture { get; set; } = "tr-TR";
    public string? LogoUrl { get; set; }
    public string? BillingNotes { get; set; }

    public SubscriptionPlanEntity SubscriptionPlan { get; set; } = null!;
    public ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();

    public SubscriptionHistory? GetCurrentSubscription()
    {
        return SubscriptionHistories
            .Where(sh => sh.IsActive && !sh.IsDeleted)
            .OrderByDescending(sh => sh.StartDate)
            .FirstOrDefault();
    }

    public bool HasValidSubscription()
    {
        var current = GetCurrentSubscription();
        if (current == null) return false;

        return current.EndDate.HasValue && current.EndDate.Value > DateTime.UtcNow;
    }

    public bool CanAddVehicle()
    {
        if (SubscriptionPlan == null)
            throw new InvalidOperationException("Subscription plan not loaded");

        return SubscriptionPlan.SupportsVehicleCount(CurrentVehicleCount + 1);
    }

    public decimal CalculateMonthlyPrice()
    {
        if (SubscriptionPlan == null)
            throw new InvalidOperationException("Subscription plan not loaded");

        return SubscriptionPlan.CalculateMonthlyPrice(CurrentVehicleCount);
    }    
}
