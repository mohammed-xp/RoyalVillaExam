using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Services
{
    public class ImageService(IWebHostEnvironment webHostEnvironment) : IImageService
    {
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] AllowedExtension =
        {
            ".jpg",
            ".jpeg",
            ".png"
        };
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return false;
                }
                var fileName = Path.GetFileName(imageUrl);

                var filePath = Path.Combine(webHostEnvironment.WebRootPath, "images", "villas", fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (!ValidateImage(file))
                {
                    throw new InvalidOperationException("Invalid image file");
                }

                var uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "images", "villas");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileExtention = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtention}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                //save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/images/villas/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool ValidateImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                return false;
            }

            var extention = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtension.Contains(extention))
            {
                return false;
            }

            return true;
        }
    }
}
