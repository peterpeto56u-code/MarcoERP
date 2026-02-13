namespace MarcoERP.Domain;

/// <summary>
/// ثوابت المجال المركزية – تمنع تكرار السلاسل النصية السحرية (DEV-08).
/// </summary>
public static class DomainConstants
{
    /// <summary>اسم مستخدم النظام الافتراضي.</summary>
    public const string SystemUser = "System";

    /// <summary>اسم حساب المدير الرئيسي.</summary>
    public const string AdminUsername = "admin";

    /// <summary>بادئة أكواد القيود المسودة.</summary>
    public const string DraftCodePrefix = "DRAFT-";

    /// <summary>الأهمية الافتراضية.</summary>
    public const string DefaultPriority = "Medium";
}
