using System;
using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Sales
{
    // ── PriceList DTOs ───────────────────────────────────────

    /// <summary>DTO for PriceList listing.</summary>
    public sealed class PriceListListDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public int TierCount { get; set; }
    }

    /// <summary>Full PriceList DTO with tiers.</summary>
    public sealed class PriceListDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public List<PriceTierDto> Tiers { get; set; } = new();
    }

    /// <summary>DTO for a single PriceTier.</summary>
    public sealed class PriceTierDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>DTO for creating a PriceList.</summary>
    public sealed class CreatePriceListDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public List<CreatePriceTierDto> Tiers { get; set; } = new();
    }

    /// <summary>DTO for creating a PriceTier.</summary>
    public sealed class CreatePriceTierDto
    {
        public int ProductId { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>DTO for updating a PriceList.</summary>
    public sealed class UpdatePriceListDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public List<CreatePriceTierDto> Tiers { get; set; } = new();
    }
}
