using System;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Reports;
using MarcoERP.Application.Interfaces.Reports;
using Microsoft.Win32;

namespace MarcoERP.WpfUI.Common
{
    /// <summary>
    /// Helper for exporting reports to PDF/Excel with SaveFileDialog.
    /// </summary>
    public static class ReportExportHelper
    {
        /// <summary>
        /// Shows a Save dialog for PDF and exports the report.
        /// Returns null on cancel, file path on success, or error message.
        /// </summary>
        public static async Task<string> ExportPdfAsync(IReportExportService exportService, ReportExportRequest request)
        {
            var dlg = new SaveFileDialog
            {
                Title = "تصدير PDF",
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = $"{request.Title}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (dlg.ShowDialog() != true)
                return null;

            var result = await exportService.ExportToPdfAsync(request, dlg.FileName, CancellationToken.None);
            return result.IsSuccess ? result.Data : result.ErrorMessage;
        }

        /// <summary>
        /// Shows a Save dialog for Excel and exports the report.
        /// Returns null on cancel, file path on success, or error message.
        /// </summary>
        public static async Task<string> ExportExcelAsync(IReportExportService exportService, ReportExportRequest request)
        {
            var dlg = new SaveFileDialog
            {
                Title = "تصدير Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"{request.Title}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return null;

            var result = await exportService.ExportToExcelAsync(request, dlg.FileName, CancellationToken.None);
            return result.IsSuccess ? result.Data : result.ErrorMessage;
        }
    }
}
