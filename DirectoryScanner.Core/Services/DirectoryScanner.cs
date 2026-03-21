using DirectoryScanner.Core.Models;
using System.Collections.Concurrent;

namespace DirectoryScanner.Core.Services;

public class DirectoryScanner
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentBag<Task> _tasks = new();
    
    public DirectoryScanner(int maxConcurrency = 4)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    public async Task<FileSystemItem> ScanAsync(string path, CancellationToken token)
    {
        var root = new FileSystemItem
        {
            Name = Path.GetFileName(path),
            IsDirectory = true
        };

        await ProcessDirectoryAsync(root, path, token);

        await Task.WhenAll(_tasks); 

        CalculateDirectorySizes(root);
        CalculatePercentages(root);
        
        CalculateDirectorySizes(root);
        
        CalculatePercentages(root);

        return root;
    }
    
    private async Task ProcessDirectoryAsync(FileSystemItem node, string path, CancellationToken token)
    {
        await _semaphore.WaitAsync(token);

        try
        {
            var dirInfo = new DirectoryInfo(path);
            
            foreach (var file in dirInfo.GetFiles())
            {
                token.ThrowIfCancellationRequested();
                
                if (file.LinkTarget != null)
                    continue;

                var fileItem = new FileSystemItem
                {
                    Name = file.Name,
                    Size = file.Length,
                    IsDirectory = false
                };

                lock (node)
                {
                    node.Children.Add(fileItem);
                    node.Size += file.Length;
                }
            }
            
            foreach (var dir in dirInfo.GetDirectories())
            {
                token.ThrowIfCancellationRequested();

                if (dir.LinkTarget != null)
                    continue;

                var dirItem = new FileSystemItem
                {
                    Name = dir.Name,
                    IsDirectory = true
                };

                lock (node)
                {
                    node.Children.Add(dirItem);
                }
                
                var task = Task.Run(() => ProcessDirectoryAsync(dirItem, dir.FullName, token), token);
                _tasks.Add(task);
            }
        }
        finally
        {
            _semaphore.Release();
        }
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