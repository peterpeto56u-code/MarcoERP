namespace MarcoERP.Application.DTOs.Inventory
{
    // ════════════════════════════════════════════════════════════
    //  Category DTOs
    // ════════════════════════════════════════════════════════════

    public sealed class CategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    public sealed class CreateCategoryDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public int? ParentCategoryId { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
    }

    public sealed class UpdateCategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
    }
}
