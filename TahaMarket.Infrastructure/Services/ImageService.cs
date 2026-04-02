using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

public class ImageService
{
    private readonly IWebHostEnvironment _env;

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImage(IFormFile image, string folder)
    {
        var folderPath = Path.Combine(_env.WebRootPath, folder);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);

        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await image.CopyToAsync(stream);

        return $"/{folder}/{fileName}";
    }
}