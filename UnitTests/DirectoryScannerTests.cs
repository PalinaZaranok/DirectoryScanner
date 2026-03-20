namespace UnitTests;

using DirectoryScanner.Core.Services;
using Xunit;

public class DirectoryScannerTests
{
    [Fact]
    public async Task ScanAsync_ShouldReturnResult()
    {
        var scanner = new DirectoryScanner();

        var result = await scanner.ScanAsync(
            Directory.GetCurrentDirectory(),
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.True(result.Children.Count > 0);
    }
    
    [Fact]
    public async Task ScanAsync_ShouldCalculateFileSizeCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var filePath = Path.Combine(tempDir, "test.txt");
        await File.WriteAllTextAsync(filePath, "12345"); // 5 байт

        var scanner = new DirectoryScanner();
        var result = await scanner.ScanAsync(tempDir, CancellationToken.None);

        Assert.Equal(5, result.Size);

        Directory.Delete(tempDir, true);
    }
    
    [Fact]
    public async Task ScanAsync_ShouldCancel()
    {
        var scanner = new DirectoryScanner();
        var cts = new CancellationTokenSource();

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await scanner.ScanAsync(
                Directory.GetCurrentDirectory(),
                cts.Token
            );
        });
    }
}