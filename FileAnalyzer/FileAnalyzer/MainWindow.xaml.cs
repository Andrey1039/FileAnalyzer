using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using FileAnalyzer.Data;
using System.Collections;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;

namespace FileAnalyzer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Добавление файлов в список анализа
        private void AddFiles(string[] files)
        {
            foreach (string path in files)
                if (Directory.Exists(path))
                {
                    string[] newFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                    foreach (string newFile in newFiles)
                        if (!FilesListBox.Items.Contains(newFile))
                            FilesListBox.Items.Add(newFile);
                }
                else
                    if (!FilesListBox.Items.Contains(path))
                    FilesListBox.Items.Add(path);
        }

        // Выбор файлов для анализа
        private void SelectFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Выберите файлы";
            openFileDialog.Filter = "Все файлы (*.*)|*.*";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
                AddFiles(openFileDialog.FileNames);
        }

        private void ItemAddBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectFileDialog();
        }

        // Удаление файлов из списка анализа
        private void DeleteFile()
        {
            IList selectedItems = FilesListBox.SelectedItems;

            if (selectedItems.Count > 0)
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                    FilesListBox.Items.Remove(selectedItems[i]);

            ResultTB.Text = string.Empty;
        }

        private void ItemDelBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }

        // Запуск анализа файлов
        private void AnalyzeStart(bool mode)
        {
            Dispatcher.Invoke(() =>
            {
                EncryptProcess.Visibility = Visibility.Visible;
                EncryptProcessPB.IsIndeterminate = true;
            });

            List<string> files = FilesListBox.Items
                .Cast<String>()
                .ToList();

            string analyzeResult = string.Empty;

            if (mode)
                analyzeResult = SignatureAnalyze.Analyze(files);
            else
                analyzeResult = HeuristicAnalyze.Analyze(files);

            Dispatcher.Invoke(() =>
            {
                ResultTB.Text = analyzeResult;
                EncryptProcess.Visibility = Visibility.Hidden;
                EncryptProcessPB.IsIndeterminate = true;
            });
        }

        private void AnalyzeStartBtn_Click(object sender, RoutedEventArgs e)
        {
            bool mode = Convert.ToBoolean(SignatureModeBtn.IsChecked);
            Task.Factory.StartNew(() => AnalyzeStart(mode));
        }

        // Добавление файлов через Drag&Drop
        private void DropInfo_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop));
                AddFiles(files);                
            }

            DropInfo.Visibility = Visibility.Hidden;
        }

        private void DropInfo_DragLeave(object sender, DragEventArgs e)
        {
            DropInfo.Visibility = Visibility.Hidden;
        }

        private void FilesListBox_DragOver(object sender, DragEventArgs e)
        {
            DropInfo.Visibility = Visibility.Visible;
        }

        // Реакция на нопку delete
        private void FilesListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteFile();
                e.Handled = true;
            }
        }

        // Реакция на ctrl+A
        private void FilesListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A)
            {
                ListBox listBox = (ListBox)sender;
                listBox.SelectAll();
                e.Handled = true;
            }
        }
    }
}