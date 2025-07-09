using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

namespace mylittle_project.infrastructure.Services
{
    public class KycService : IKycService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public KycService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task AddDocumentRequestAsync(KycDocumentRequestDto dto)
        {
            var request = new KycDocumentRequest
            {
                DealerId = dto.DealerId,
                DocType = dto.DocType,
                IsRequired = dto.IsRequired
            };

            _unitOfWork.KycDocumentRequests.Add(request);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<KycDocumentRequestDto>> GetRequestedDocumentsAsync(Guid dealerId)
        {
            return await _unitOfWork.KycDocumentRequests
                .Find(k => k.DealerId == dealerId)
                .Select(k => new KycDocumentRequestDto
                {
                    DealerId = k.DealerId,
                    DocType = k.DocType,
                    IsRequired = k.IsRequired
                })
                .ToListAsync();
        }

        public async Task<string> UploadDocumentAsync(KycDocumentUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("No file provided.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedKycDocs", dto.DealerId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, $"{dto.DocType}_{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var uploadedDoc = new KycDocumentUpload
            {
                DealerId = dto.DealerId,
                DocType = dto.DocType,
                FileUrl = filePath,
                UploadedAt = DateTime.UtcNow
            };

            _unitOfWork.KycDocumentUploads.Add(uploadedDoc);
            await _unitOfWork.SaveAsync();

            return filePath;
        }
    }
}