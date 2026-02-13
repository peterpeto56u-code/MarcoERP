using System;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Domain.Enums;

namespace MarcoERP.WpfUI.Services
{
    public sealed record QuickTreasuryDialogRequest(
        string Title,
        DateTime VoucherDate,
        decimal DefaultAmount,
        string Description,
        string Notes,
        QuickTreasuryDialogKind Kind);

    public enum QuickTreasuryDialogKind
    {
        Receipt,
        Payment
    }

    public sealed record QuickTreasuryDialogResult(
        int CashboxId,
        PaymentMethod PaymentMethod,
        decimal Amount);

    public interface IQuickTreasuryDialogService
    {
        Task<QuickTreasuryDialogResult> ShowAsync(QuickTreasuryDialogRequest request, CancellationToken ct = default);
    }
}
