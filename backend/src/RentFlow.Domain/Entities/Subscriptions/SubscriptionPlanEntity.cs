using RentFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentFlow.Domain.Entities.Subscriptions
{
    public class SubscriptionPlanEntity: BaseAuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int MinVehicles { get; set; }

        public int? MaxVehicles { get; set; }

        public decimal MonthlyPrice { get; set; }

        public decimal AnnualPrice { get; set; }

        public decimal? PerVehiclePrice { get; set; }

        public bool IsDemo { get; set; }

        public int TrialDays { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public string? Features { get; set; }

        public ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();

        public decimal CalculateMonthlyPrice(int vehicleCount)
        {
            if (vehicleCount < MinVehicles)
                throw new ArgumentException($"Vehicle count must be at least {MinVehicles}");

            if (MaxVehicles.HasValue && vehicleCount > MaxVehicles.Value)
            {
                // If there's a per-vehicle price, charge for overages
                if (PerVehiclePrice.HasValue)
                {
                    var overage = vehicleCount - MaxVehicles.Value;
                    return MonthlyPrice + (overage * PerVehiclePrice.Value);
                }

                throw new ArgumentException($"Vehicle count exceeds maximum of {MaxVehicles.Value}");
            }

            return MonthlyPrice;
        }

        public bool SupportsVehicleCount(int vehicleCount)
        {
            return vehicleCount >= MinVehicles
                   && (!MaxVehicles.HasValue || vehicleCount <= MaxVehicles.Value || PerVehiclePrice.HasValue);
        }
    }
}
