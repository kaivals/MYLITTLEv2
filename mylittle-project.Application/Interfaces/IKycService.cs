using mylittle_project.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Application.Interfaces
{
    public interface IKycService
    {
        Task AddDocumentRequestAsync(KycDocumentRequestDto dto);
        Task<List<KycDocumentRequestDto>> GetRequestedDocumentsAsync(Guid dealerId);
        Task<string> UploadDocumentAsync(KycDocumentUploadDto dto);
        Task<bool> SoftDeleteKycDocumentAsync(Guid documentId);
        Task<byte[]?> DownloadDocumentAsync(Guid documentId);
        Task<bool> RestoreKycDocumentAsync(Guid documentId);

    }
}
