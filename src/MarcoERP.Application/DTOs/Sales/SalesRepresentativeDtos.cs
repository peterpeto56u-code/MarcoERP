namespace MarcoERP.Application.DTOs.Sales
{
    public sealed class SalesRepresentativeDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public decimal CommissionRate { get; set; }
        public int CommissionBasedOn { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }

    public sealed class CreateSalesRepresentativeDto
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public decimal CommissionRate { get; set; }
        public int CommissionBasedOn { get; set; }
        public string Notes { get; set; }
    }

    public sealed class UpdateSalesRepresentativeDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public decimal CommissionRate { get; set; }
        public int CommissionBasedOn { get; set; }
        public string Notes { get; set; }
    }

    public sealed class SalesRepresentativeSearchResultDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string Phone { get; set; }
        public decimal CommissionRate { get; set; }
        public bool IsActive { get; set; }
    }
}
