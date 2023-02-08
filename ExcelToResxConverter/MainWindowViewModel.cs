using ExcelToResxConverter.Service;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using ReactiveUI;
using System;
using System.IO;
using System.Windows;

namespace ExcelToResxConverter
{
    public class MainWindowViewModel : ReactiveObject
    {
        #region Const Field
        public const string KOREAN_FILE_NAME = "Resources.ko-KR.resx";
        public const string ENGLISH_FILE_NAME = "Resources.en-US.resx";
        public const string TEMPLATE_FILE_NAME = "template.xml";
        #endregion

        #region Internal Field
        private string _excelFilePath;
        private string _resxDirectoryPath;
        #endregion

        #region Properties
        public string ExcelFilePath
        {
            get => _excelFilePath;
            set => this.RaiseAndSetIfChanged(ref _excelFilePath, value);
        }

        public string ResxDirectoryPath
        {
            get => _resxDirectoryPath;
            set => this.RaiseAndSetIfChanged(ref _resxDirectoryPath, value);
        }
        #endregion

        #region Command
        public DelegateCommand SelectExcelFileCommand { get; }
        public DelegateCommand SelectResxFileCommand { get; }
        public DelegateCommand ConvertCommand { get; }
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            SelectExcelFileCommand = new DelegateCommand(SelectExcelFile);
            SelectResxFileCommand = new DelegateCommand(SelectResxFile);

            ConvertCommand = new DelegateCommand(Convert);
        }
        #endregion

        #region Functions
        private void Convert()
        {
            if (!IsValidFilePath(ExcelFilePath))
            {
                MessageBox.Show("Excel file path is invalid.");
                return;
            }

            if(!IsValidDirectoryPath(ResxDirectoryPath))
            {
                MessageBox.Show("Directory for resx output is invalid.");
                return;
            }

            try
            {
                var units = ExcelReader.Read(ExcelFilePath);
                var builder = new ResxBuilder();

                builder.TemplatePath = TEMPLATE_FILE_NAME;
                builder.Units.AddRange(units);

                var korean = builder.BuildKorean();
                var english = builder.BuildEnglish();

                korean.Save(ResxDirectoryPath + $@"\{KOREAN_FILE_NAME}");
                english.Save(ResxDirectoryPath + $@"\{ENGLISH_FILE_NAME}");
            }
            catch(Exception e)
            {
                MessageBox.Show($"Exporting Fail\n{e.Message}");
                return;
            }

            MessageBox.Show("Success exporting");
        }

        private static bool IsValidFilePath(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        private static bool IsValidDirectoryPath(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        private void SelectResxFile()
        {
            var dialog = new VistaFolderBrowserDialog()
            {
                UseDescriptionForTitle = true,
                Description = "Directory to save resx file.",
            };

            if (dialog.ShowDialog() != true)
                return;

            ResxDirectoryPath = dialog.SelectedPath;
        }

        private void SelectExcelFile()
        {
            var dialog = new VistaOpenFileDialog()
            {
                Title = "Select Excel File",
                Filter = "Excel File (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                Multiselect = false,
                CheckFileExists = true,
            };

            if (dialog.ShowDialog() != true)
                return;

            ExcelFilePath = dialog.FileName;
        }
        #endregion
    }
}
