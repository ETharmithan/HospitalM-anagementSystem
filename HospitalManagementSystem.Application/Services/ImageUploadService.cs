using HospitalManagementSystem.Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly string _uploadDirectory;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImageUploadService(string uploadDirectory = "wwwroot/uploads/patients")
        {
            _uploadDirectory = uploadDirectory;
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                // Validate file
                if (!IsValidImageFile(fileName, fileStream.Length))
                {
                    throw new InvalidOperationException("Invalid file. Only images (jpg, jpeg, png, gif, webp) up to 5MB are allowed.");
                }

                // Generate unique filename
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
                string filePath = Path.Combine(_uploadDirectory, uniqueFileName);

                // Save file
                using (var fileToSave = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileToSave);
                }

                // Return relative path for storage in database
                return $"/uploads/patients/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                // Remove leading slash if present
                string relativePath = imagePath.StartsWith("/") ? imagePath.Substring(1) : imagePath;
                string fullPath = Path.Combine("wwwroot", relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting image: {ex.Message}", ex);
            }
        }

        public bool IsValidImageFile(string fileName, long fileSize)
        {
            // Check file size
            if (fileSize > _maxFileSize)
                return false;

            // Check file extension
            string extension = Path.GetExtension(fileName).ToLower();
            return _allowedExtensions.Contains(extension);
        }
    }
}
