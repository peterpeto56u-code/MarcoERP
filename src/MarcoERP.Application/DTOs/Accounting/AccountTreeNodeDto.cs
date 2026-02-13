using System.Collections.Generic;

namespace MarcoERP.Application.DTOs.Accounting
{
    /// <summary>
    /// Hierarchical tree node DTO for displaying the Chart of Accounts as a tree.
    /// Each node can have children.
    /// </summary>
    public sealed class AccountTreeNodeDto
    {
        public int Id { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public string AccountTypeName { get; set; }
        public int Level { get; set; }
        public bool IsLeaf { get; set; }
        public bool AllowPosting { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemAccount { get; set; }
        public List<AccountTreeNodeDto> Children { get; set; } = new();
    }
}
