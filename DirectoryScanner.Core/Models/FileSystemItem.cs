using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DirectoryScanner.Core.Models;

public class FileSystemItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private long _size;
    private double _percentage;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public long Size
    {
        get => _size;
        set
        {
            _size = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public bool IsDirectory { get; set; }

    public ObservableCollection<FileSystemItem> Children { get; set; }
        = new ObservableCollection<FileSystemItem>();

    public string DisplayName => $"{Name} ({Size} bytes, {Percentage:F2}%)";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}