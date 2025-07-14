using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;

namespace mylittle_project.Infrastructure.Services
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
        public async Task<bool> RestoreKycDocumentAsync(Guid documentId)
        {
            var document = await _unitOfWork.KycDocumentUploads.GetByIdAsync(documentId);
            if (document == null || !document.IsDeleted)
                return false;

            document.IsDeleted = false;
            await _unitOfWork.SaveAsync();
            return true;
        }


        public async Task<bool> SoftDeleteKycDocumentAsync(Guid documentId)
        {
            var doc = await _unitOfWork.KycDocumentUploads.GetByIdAsync(documentId);
            if (doc == null || doc.IsDeleted) return false;

            doc.IsDeleted = true;
            doc.DeletedAt = DateTime.UtcNow;

            _unitOfWork.KycDocumentUploads.Update(doc);
            await _unitOfWork.SaveAsync();
            return true;
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
        public async Task<byte[]?> DownloadDocumentAsync(Guid documentId)
        {
            var doc = await _unitOfWork.KycDocumentUploads
                .Find(x => x.Id == documentId && !x.IsDeleted)
                .FirstOrDefaultAsync();

            if (doc == null || !File.Exists(doc.FileUrl))
                return null;

            return await File.ReadAllBytesAsync(doc.FileUrl);
        }

        public async Task<string> UploadDocumentAsync(KycDocumentUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("No file provided.");

            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "UploadedKycDocs", dto.DealerId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{dto.DocType}_{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

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
