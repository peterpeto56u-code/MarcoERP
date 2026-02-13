namespace MarcoERP.Application.DTOs.Inventory
{
    // ════════════════════════════════════════════════════════════
    //  Unit DTOs
    // ════════════════════════════════════════════════════════════

    public sealed class UnitDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string AbbreviationAr { get; set; }
        public string AbbreviationEn { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class CreateUnitDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string AbbreviationAr { get; set; }
        public string AbbreviationEn { get; set; }
    }

    public sealed class UpdateUnitDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string AbbreviationAr { get; set; }
        public string AbbreviationEn { get; set; }
    }
}
