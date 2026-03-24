using DirectoryScanner.Core.Models;

namespace DirectoryScanner.Core.Services;

public interface IDirectoryScanner
{
    Task ScanAsync(FileSystemItem root, string path, CancellationToken token);
}