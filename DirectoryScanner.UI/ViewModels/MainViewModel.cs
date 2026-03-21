using System.Collections.ObjectModel;
using System.IO;
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

        var root = new FileSystemItem { Name = "Loading...", IsDirectory = true };
        Items.Add(root);

        try
        {
            var result = await _scanner.ScanAsync(selectedPath, _cts.Token);

            Items[0] = result;
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    private void CancelScan()
    {
        _cts?.Cancel();
    }
}