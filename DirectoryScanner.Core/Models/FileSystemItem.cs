namespace DirectoryScanner.Core.Models;

public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;

    public long Size { get; set; }

    public double Percentage { get; set; } // процент от родителя

    public bool IsDirectory { get; set; }

    public List<FileSystemItem> Children { get; set; } = new();  //вложенность
    
    public string DisplayName => $"{Name} ({Size} bytes, {Percentage:F2}%)";
}