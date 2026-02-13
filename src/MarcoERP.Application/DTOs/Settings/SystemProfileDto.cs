namespace MarcoERP.Application.DTOs.Settings
{
    /// <summary>
    /// Read-only DTO for SystemProfile entity.
    /// Phase 3: Progressive Complexity Layer.
    /// </summary>
    public sealed class SystemProfileDto
    {
        public int Id { get; set; }
        public string ProfileName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
