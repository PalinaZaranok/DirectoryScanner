using DirectoryScanner.Core.Models;

namespace UnitTests;

public class DirectoryScannerTests
{
    [Fact]
    public async Task ScanAsync_ShouldReturnResult()
    {
        var tempDir = CreateTempDirectory();

        await File.WriteAllTextAsync(
            Path.Combine(tempDir, "test.txt"),
            "data"
        );

        var root = new FileSystemItem { Name = "Test", IsDirectory = true };
        var scanner = new DirectoryScanner.Core.Services.DirectoryScanner();

        await scanner.ScanAsync(
            root,
            tempDir,
            CancellationToken.None,
            (parent, child) =>
            {
                parent.Children.Add(child); 
            });

        Assert.NotNull(root);
        Assert.NotEmpty(root.Children);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ScanAsync_ShouldCalculateFileSizeCorrectly()
    {
        var tempDir = CreateTempDirectory();

        var filePath = Path.Combine(tempDir, "test.txt");
        await File.WriteAllTextAsync(filePath, "12345"); 

        var root = new FileSystemItem { Name = "Test", IsDirectory = true };
        var scanner = new DirectoryScanner.Core.Services.DirectoryScanner();

        await scanner.ScanAsync(
            root,
            tempDir,
            CancellationToken.None,
            (parent, child) =>
            {
                parent.Children.Add(child);
            });
        
        CalculateDirectorySizes(root);

        Assert.Equal(5, root.Size);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ScanAsync_ShouldCancel()
    {
        var tempDir = CreateTempDirectory();

        var scanner = new DirectoryScanner.Core.Services.DirectoryScanner();
        var cts = new CancellationTokenSource();

        cts.Cancel();

        var root = new FileSystemItem { Name = "Test", IsDirectory = true };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await scanner.ScanAsync(
                root,
                tempDir,
                cts.Token,
                (parent, child) =>
                {
                    parent.Children.Add(child);
                });
        });

        Directory.Delete(tempDir, true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(100)]
    public async Task ScanAsync_ShouldWorkWithDifferentWorkerCounts(int workers)
    {
        var tempDir = CreateTempDirectory();

        for (int i = 0; i < 10; i++)
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, $"file{i}.txt"),
                "data"
            );
        }

        var scanner = new DirectoryScanner.Core.Services.DirectoryScanner(workers);
        var root = new FileSystemItem { Name = "Test", IsDirectory = true };

        await scanner.ScanAsync(
            root,
            tempDir,
            CancellationToken.None,
            (parent, child) =>
            {
                parent.Children.Add(child);
            });

        CalculateDirectorySizes(root);

        Assert.True(root.Size > 0);
        Assert.NotEmpty(root.Children);

        Directory.Delete(tempDir, true);
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
    
    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }
}