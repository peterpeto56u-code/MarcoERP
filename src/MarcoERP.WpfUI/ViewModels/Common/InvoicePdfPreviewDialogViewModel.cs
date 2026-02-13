using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels;

namespace MarcoERP.WpfUI.ViewModels.Common
{
    public sealed class InvoicePdfPreviewDialogViewModel : BaseViewModel
    {
        private string _titleText;
        public string TitleText { get => _titleText; set => SetProperty(ref _titleText, value); }

        private string _statusText;
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private string _pdfPath;
        public string PdfPath
        {
            get => _pdfPath;
            set
            {
                if (SetProperty(ref _pdfPath, value))
                    RelayCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanOpenPdf => !string.IsNullOrWhiteSpace(PdfPath) && File.Exists(PdfPath);

        public ICommand CloseCommand { get; }
        public ICommand OpenPdfCommand { get; }

        public event Action RequestClose;

        public InvoicePdfPreviewDialogViewModel()
        {
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
            OpenPdfCommand = new RelayCommand(_ => OpenPdf(), _ => CanOpenPdf);
        }

        private void OpenPdf()
        {
            try
            {
                if (!CanOpenPdf)
                    return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = PdfPath,
                    UseShellExecute = true
                });
            }
            catch
            {
                StatusText = "تعذر فتح ملف PDF.";
            }
        }
    }
}
