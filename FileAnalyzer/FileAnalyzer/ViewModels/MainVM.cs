using System;
using System.IO;
using System.Linq;
using System.Windows;
using FileAnalyzer.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using FileAnalyzer.Data.Services;
using System.Collections.Generic;
using FileAnalyzer.Models.Analyze;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace FileAnalyzer.ViewModels
{
    internal class MainVM : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Item> _filePaths;
        private readonly IDialogService _dialogService;
        private readonly IMessengerService _messengerService;
        private bool _dropInfoVisible;
        private bool _tProcess;
        private string _endText;

        public MainVM()
        {
            _dropInfoVisible = false;
            _endText = string.Empty;
            _tProcess = false;
            _filePaths = new ObservableCollection<Item>();
            _dialogService = new DialogService();
            _messengerService = new MessengerService();
        }

        // Добавление элементов в список файлов
        private void AddValue(string[] files)
        {
            foreach (string path in files)
                if (Directory.Exists(path))
                {
                    foreach (string newFile in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        if (!FilePaths.Any(x => x.FileName == newFile))
                            FilePaths.Add(new Item(newFile, false));
                    }
                }
                else
                    if (!FilePaths.Any(x => x.FileName == path))
                    FilePaths.Add(new Item(path, false));
        }

        // Команда добавления элементов (кнопка)
        private RelayCommand? addItemCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addItemCommand ??= new RelayCommand(_ => 
                {
                    AddValue(_dialogService.OpenFileDialog("Все файлы (*.*)|*.*", true));
                    EndText = string.Empty;
                });
            }
        }

        // Команда удаления элементов из списка файлов
        private RelayCommand? removeItemCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return removeItemCommand ??= new RelayCommand(_ =>
                  {
                      List<Item> selectedItems = FilePaths.Where(x => x.IsChecked).ToList();

                      if (selectedItems.Count > 0)
                      {
                          for (int i = selectedItems.Count - 1; i >= 0; i--)
                              FilePaths.Remove(selectedItems[i]);

                          EndText = string.Empty;
                      }
                  });
            }
        }

        // Команда выбора всех элементов
        private RelayCommand? selectAllCommand;
        public RelayCommand SelectAllCommand
        {
            get
            {
                return selectAllCommand ??= new RelayCommand(_ =>
                    FilePaths.Where(x => !x.IsChecked).ToList().ForEach(y => y.IsChecked = true));
            }
        }

        // Обработка файлов
        private void AnalyzeFile(bool mode)
        {
            List<string> files = FilePaths.Select(x => x.FileName).ToList();

            try
            {
                if (mode)
                    EndText = SignatureAnalyze.Analyze(files);
                else
                    EndText = HeuristicAnalyze.Analyze(files);
            }
            catch
            {
                _messengerService.ShowErrorMessageBox($"При обработке файлов произошла ошибка");
            }
        }

        // Команда анализа файлов
        private RelayCommand? analyzeDataCommand;
        public RelayCommand AnalyzeDataCommand
        {
            get
            {
                return analyzeDataCommand ??= new RelayCommand(mode =>
                  {
                      TProcess = true;

                      Task.Factory.StartNew(() =>
                      {
                          AnalyzeFile((bool)mode);
                          TProcess = false;
                      });
                  });
            }
        }

        // Добавление элементов в список файлов (Drag&Drop)
        private RelayCommand? dragCommand;
        public RelayCommand DragCommand
        {
            get
            {
                return dragCommand ??= new RelayCommand(param =>
                  {
                      DragEventArgs args = (DragEventArgs)param;

                      if (args.Data.GetDataPresent(DataFormats.FileDrop))
                          AddValue((string[])args.Data.GetData(DataFormats.FileDrop));

                      DropInfoVisible = false;
                  });
            }
        }

        // Команда отображения поля Drag&Drop
        private RelayCommand? dragShowCommand;
        public RelayCommand DragShowCommand
        {
            get
            {
                return dragShowCommand ??= new RelayCommand(isVisible =>
                    DropInfoVisible = Convert.ToBoolean(isVisible));
            }
        }

        public bool TProcess
        {
            get { return _tProcess; }
            set
            {
                _tProcess = value;
                OnPropertyChanged("TProcess");
            }
        }

        public bool DropInfoVisible
        {
            get { return _dropInfoVisible; }
            set
            {
                _dropInfoVisible = value;
                OnPropertyChanged("DropInfoVisible");
            }
        }

        public string EndText
        {
            get { return _endText; }
            set
            {
                _endText = value;
                OnPropertyChanged("EndText");
            }
        }

        public ObservableCollection<Item> FilePaths
        {
            get
            {
                return _filePaths;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
