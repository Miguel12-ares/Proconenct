using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;

namespace ProConnect.Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private const int MaxFilesPerUser = 10;
        private readonly string _portfolioRoot = Path.Combine("wwwroot", "portfolio");

        // Aquí se debe inyectar el repositorio de portafolio y cualquier dependencia necesaria
        private readonly IPortfolioRepository _portfolioRepository;

        public PortfolioService(IPortfolioRepository portfolioRepository)
        {
            _portfolioRepository = portfolioRepository;
        }

        public async Task<PortfolioFileDto> UploadPortfolioFileAsync(string userId, IFormFile file, string description)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Archivo no valido");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException("Tipo de archivo no permitido");

            if (file.Length > MaxFileSize)
                throw new ArgumentException("El archivo excede el tamano maximo permitido (5MB)");

            var userFiles = await _portfolioRepository.GetFilesByUserAsync(userId);
            if (userFiles.Count >= MaxFilesPerUser)
                throw new InvalidOperationException("Limite de archivos alcanzado");

            var userFolder = Path.Combine(_portfolioRoot, userId);
            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(userFolder, uniqueName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/portfolio/{userId}/{uniqueName}";
            var entity = new PortfolioFile
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                Url = url,
                Description = description,
                UploadedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _portfolioRepository.AddFileAsync(entity);

            return new PortfolioFileDto
            {
                Id = entity.Id,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                Size = entity.Size,
                Url = entity.Url,
                Description = entity.Description,
                UploadedAt = entity.UploadedAt,
                UserId = entity.UserId
            };
        }

        public async Task<List<PortfolioFileDto>> GetPortfolioFilesAsync(string userId)
        {
            var files = await _portfolioRepository.GetFilesByUserAsync(userId);
            return files.ConvertAll(entity => new PortfolioFileDto
            {
                Id = entity.Id,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                Size = entity.Size,
                Url = entity.Url,
                Description = entity.Description,
                UploadedAt = entity.UploadedAt,
                UserId = entity.UserId
            });
        }

        public async Task<bool> DeletePortfolioFileAsync(string userId, string fileId)
        {
            var file = await _portfolioRepository.GetFileByIdAsync(userId, fileId);
            if (file == null) return false;
            var filePath = Path.Combine(_portfolioRoot, userId, Path.GetFileName(file.Url));
            if (File.Exists(filePath)) File.Delete(filePath);
            await _portfolioRepository.DeleteFileAsync(userId, fileId);
            return true;
        }

        public async Task<bool> UpdatePortfolioFileDescriptionAsync(string userId, string fileId, string description)
        {
            return await _portfolioRepository.UpdateFileDescriptionAsync(userId, fileId, description);
        }
    }
} 