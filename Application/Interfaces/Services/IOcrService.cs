namespace Application.Interfaces.Services
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(string filePath);
    }
}
