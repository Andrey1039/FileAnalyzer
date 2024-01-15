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
using FileAnalyzer.ViewModels;

namespace FileAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();
        }
    }
}
