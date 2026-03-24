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

    public async Task ScanAsync(
        FileSystemItem root,
        string path,
        CancellationToken token,
        Action<FileSystemItem, FileSystemItem>? onItemFound)
    {
        
        _tasks.Clear();
        await ProcessDirectoryAsync(root, path, token, onItemFound);

        try
        {
            await Task.WhenAll(_tasks);
        }
        catch (OperationCanceledException)
        {
            
        }
        
        //CalculateDirectorySizes(root);
        //CalculatePercentages(root);
    }

    private async Task ProcessDirectoryAsync(FileSystemItem node, string path,
        CancellationToken token, Action<FileSystemItem, FileSystemItem>? onItemFound)
    {
        await _semaphore.WaitAsync(token);

        try
        {
            token.ThrowIfCancellationRequested();

            var dirInfo = new DirectoryInfo(path);

            FileInfo[] files;
            try
            {
                files = dirInfo.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }

            foreach (var file in files)
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
                    //node.Size += file.Length;
                }
                onItemFound?.Invoke(node, fileItem);
            }

            DirectoryInfo[] dirs;
            try
            {
                dirs = dirInfo.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }

            foreach (var dir in dirs)
            {
                token.ThrowIfCancellationRequested();

                if (dir.LinkTarget != null)
                    continue;

                var dirItem = new FileSystemItem
                {
                    Name = dir.Name,
                    IsDirectory = true
                };

                
                
                onItemFound?.Invoke(node, dirItem);


                var task = Task.Run(() =>
                    ProcessDirectoryAsync(dirItem, dir.FullName, token, onItemFound), token);

                _tasks.Add(task);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /*
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
    */
}