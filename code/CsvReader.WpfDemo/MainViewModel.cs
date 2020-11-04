using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CsvReader;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    ///     This class contains properties that the main View can data bind to.
    ///     <para>
    ///         Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    ///     </para>
    ///     <para>
    ///         You can also use Blend to data bind with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm
    ///     </para>
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private DataTable _data;
        private char _separator;


        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Data = new DataTable();
            Separator = ',';
            ImportFileCommand = new Command(ImportFile);
        }

        public ICommand ImportFileCommand { get; set; }

        public DataTable Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged(nameof(Data));
                OnPropertyChanged(nameof(DataView));
            }
        }

        public DataView DataView => _data.AsDataView();

        public char Separator
        {
            get => _separator;
            set
            {
                _separator = value;
                OnPropertyChanged(nameof(Separator));
            }
        }

        private void ImportFile()
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = "*.csv";
            if (dialog.ShowDialog() != true) return;
            var fileName = Path.GetTempFileName();

            // Create new local file and copy contents of uploaded file
            using (var localFile = File.OpenWrite(fileName))
            using (var uploadedFile = dialog.OpenFile())
            {
                uploadedFile.CopyTo(localFile);
            }

            using (var csv = new CsvReader.CsvReader(new StreamReader(fileName), true, Separator))
            {
                csv.VirtualColumns.Add(new Column
                {
                    Name = "AmountOfChildren",
                    DefaultValue = "1",
                    Type = typeof(int),
                    NumberStyles = NumberStyles.Integer
                });
                csv.VirtualColumns.Add(new Column {Name = "Sex", DefaultValue = "M", Type = typeof(string)});
                IDataReader reader = csv;
                var schema = reader.GetSchemaTable();
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                Data.Clear();
                Data.Load(reader);
                OnPropertyChanged(nameof(Data));
                OnPropertyChanged(nameof(DataView));
            }

            File.Delete(fileName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}