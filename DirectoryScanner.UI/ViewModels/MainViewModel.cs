using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DirectoryScanner.Core.Models;
using DirectoryScanner.Core.Services;
using DirectoryScanner.UI.Commands;

namespace DirectoryScanner.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly Core.Services.DirectoryScanner _scanner = new DirectoryScanner.Core.Services.DirectoryScanner();

    private CancellationTokenSource? _cts;

    public ObservableCollection<FileSystemItem> Items { get; set; } = new();

    public ICommand ScanCommand { get; }
    public ICommand CancelCommand { get; }

    public MainViewModel()
    {
        ScanCommand = new RelayCommand(StartScan);
        CancelCommand = new RelayCommand(CancelScan);
    }
    
    /*
    private async void StartScan()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Выберите папку"
        };

        if (dialog.ShowDialog() != true)
            return;

        var selectedPath = Path.GetDirectoryName(dialog.FileName);

        if (string.IsNullOrEmpty(selectedPath))
            return;

        _cts = new CancellationTokenSource();

        Items.Clear();

        var root = new FileSystemItem
        {
            Name = Path.GetFileName(selectedPath),
            IsDirectory = true
        };

        Items.Add(root);

        try
        {
            await _scanner.ScanAsync(
                root, selectedPath, _cts.Token,
                (parent, child) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        parent.Children.Add(child);
                    });
                });
        }
        catch (OperationCanceledException)
        {
        }
        
        await Application.Current.Dispatcher.InvokeAsync(() => { });
        
        CalculateDirectorySizes(root);
        CalculatePercentages(root);
    }
    */
    
    private async void StartScan()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Выберите папку"
        };

        if (dialog.ShowDialog() != true)
            return;

        var selectedPath = Path.GetDirectoryName(dialog.FileName);

        if (string.IsNullOrEmpty(selectedPath))
            return;

        _cts = new CancellationTokenSource();

        Items.Clear();

        var root = new FileSystemItem
        {
            Name = Path.GetFileName(selectedPath),
            IsDirectory = true
        };

        Items.Add(root);

        try
        {
            await _scanner.ScanAsync(
                root,
                selectedPath,
                _cts.Token,
                (parent, child) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        parent.Children.Add(child);
                    });
                });
            
            await Application.Current.Dispatcher.InvokeAsync(() => { });

            CalculateDirectorySizes(root);
            CalculatePercentages(root);
            
            MessageBox.Show(
                "Сканирование завершено",
                "Готово",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => { });

            CalculateDirectorySizes(root);
            CalculatePercentages(root);
            MessageBox.Show(
                "Сканирование отменено",
                "Отмена",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void CancelScan()
    {
        _cts?.Cancel();
    }
    
    private long CalculateDirectorySizes(FileSystemItem node)
    {
        if (!node.IsDirectory)
            return node.Size;

        long total = 0;

        foreach (var child in node.Children)
        {
            total += CalculateDirectorySizes(child);
        }

        node.Size = total;
        return total;
    }

    private void CalculatePercentages(FileSystemItem node)
    {
        foreach (var child in node.Children)
        {
            if (node.Size > 0)
                child.Percentage = (double)child.Size / node.Size * 100;

            if (child.IsDirectory)
                CalculatePercentages(child);
        }
    }
}