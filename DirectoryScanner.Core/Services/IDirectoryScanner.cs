using DirectoryScanner.Core.Models;

namespace DirectoryScanner.Core.Services;

public interface IDirectoryScanner
{
    Task<FileSystemItem> ScanAsync(string path, CancellationToken cancellationToken);
}