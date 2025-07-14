using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/dealer")]
    public class DealerController : ControllerBase
    {
        private readonly IDealerService _dealerService;
        private readonly IUserDealerService _userDealerService;
        private readonly IVirtualNumberService _virtualNumberService;
        private readonly IDealerSubscriptionApplicationService _dealerSubscriptionService;
        private readonly IKycService _kycService;

        public DealerController(
            IDealerService dealerService,
            IUserDealerService userDealerService,
            IVirtualNumberService virtualNumberService,
            IDealerSubscriptionApplicationService dealerSubscriptionService,
            IKycService kycService)
        {
            _dealerService = dealerService;
            _userDealerService = userDealerService;
            _virtualNumberService = virtualNumberService;
            _dealerSubscriptionService = dealerSubscriptionService;
            _kycService = kycService;
        }

        // ──────────────── POST ENDPOINTS ────────────────

        [HttpPost("business-info")]
        public async Task<IActionResult> CreateBusinessInfo([FromBody] DealerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _dealerService.CreateBusinessInfoAsync(dto);
            return Ok(new { message = "Business Info created successfully", dealerId = id });
        }

        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody] UserDealerDto dto)
        {
            var userId = await _userDealerService.AddUserAsync(dto);
            return Ok(new { userId });
        }

        [HttpPost("user/batch")]
        public async Task<IActionResult> AddMultipleUsers([FromBody] List<UserDealerDto> users)
        {
            if (users == null || users.Count == 0)
                return BadRequest("User list is empty.");

            var createdUserIds = new List<Guid>();
            foreach (var user in users)
            {
                if (user.DealerId == Guid.Empty) continue;
                var id = await _userDealerService.AddUserAsync(user);
                createdUserIds.Add(id);
            }

            return Ok(new { message = "Users added successfully.", userIds = createdUserIds });
        }

        [HttpPost("subscription/apply")]
        public async Task<IActionResult> ApplyForSubscription([FromBody] DealerSubscriptionApplicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _dealerSubscriptionService.AddSubscriptionAsync(dto);
            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }

        [HttpPost("kyc/request")]
        public async Task<IActionResult> AddDocumentRequest([FromBody] KycDocumentRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            await _kycService.AddDocumentRequestAsync(dto);
            return Ok(new { message = "Document request added successfully." });
        }

        [HttpPost("kyc/upload")]
        public async Task<IActionResult> UploadKycDocument([FromForm] KycDocumentUploadDto dto)
        {
            if (dto.File == null)
                return BadRequest("File is required.");

            var filePath = await _kycService.UploadDocumentAsync(dto);
            return Ok(new { message = "Document uploaded successfully.", filePath });
        }

        // ──────────────── GET ENDPOINTS ────────────────

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDealers()
        {
            var dealers = await _dealerService.GetAllDealersAsync();
            return Ok(dealers);
        }

        [HttpGet("tenant")]
        public async Task<IActionResult> GetDealersByTenant([FromQuery] Guid tenantId)
        {
            var dealers = await _dealerService.GetDealersByTenantAsync(tenantId);
            return Ok(dealers);
        }

        [HttpGet("user/{dealerId}")]
        public async Task<IActionResult> GetUsers(Guid dealerId)
        {
            var users = await _userDealerService.GetUsersByDealerAsync(dealerId);
            return Ok(users);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userDealerService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("user/paginated")]
        public async Task<IActionResult> GetPaginatedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userDealerService.GetPaginatedUsersAsync(page, pageSize);
            return Ok(users);
        }

        [HttpGet("virtual-number/{dealerId}")]
        public async Task<IActionResult> GetVirtualNumber(Guid dealerId)
        {
            var number = await _virtualNumberService.GetAssignedNumberAsync(dealerId);
            return Ok(new { virtualNumber = number });
        }

        [HttpGet("subscription/tenant/{tenantId}")]
        public async Task<IActionResult> GetSubscriptionsByTenant(Guid tenantId)
        {
            var apps = await _dealerSubscriptionService.GetByTenantAsync(tenantId);
            return Ok(apps);
        }

        [HttpGet("subscription/dealer/{dealerId}")]
        public async Task<IActionResult> GetSubscriptionsByDealer(Guid dealerId)
        {
            var apps = await _dealerSubscriptionService.GetByDealerAsync(dealerId);
            return Ok(apps);
        }

        [HttpGet("kyc/requested/{dealerId}")]
        public async Task<IActionResult> GetRequestedDocuments(Guid dealerId)
        {
            var docs = await _kycService.GetRequestedDocumentsAsync(dealerId);
            return Ok(docs);
        }

        [HttpGet("kyc/download/{documentId}")]
        public async Task<IActionResult> DownloadKycDocument(Guid documentId)
        {
            var fileBytes = await _kycService.DownloadDocumentAsync(documentId);
            if (fileBytes == null)
                return NotFound(new { message = "Document not found or deleted." });

            return File(fileBytes, "application/octet-stream", $"KycDocument_{documentId}.pdf");
        }

        // ──────────────── DELETE & PATCH (Soft Delete / Restore) ────────────────

        [HttpDelete("business-info/{dealerId}")]
        public async Task<IActionResult> SoftDeleteDealer(Guid dealerId)
        {
            var result = await _dealerService.SoftDeleteDealerAsync(dealerId);
            if (!result)
                return NotFound(new { message = "Dealer not found." });

            return Ok(new { message = "Dealer soft deleted successfully." });
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> SoftDeleteUser(Guid userId)
        {
            var result = await _userDealerService.SoftDeleteUserAsync(userId);
            if (!result)
                return NotFound(new { message = "User not found or already deleted." });

            return Ok(new { message = "User soft deleted successfully." });
        }

        [HttpDelete("subscription/{subscriptionId}")]
        public async Task<IActionResult> SoftDeleteSubscription(Guid subscriptionId)
        {
            var result = await _dealerSubscriptionService.SoftDeleteAsync(subscriptionId);
            if (!result)
                return NotFound(new { message = "Subscription application not found." });

            return Ok(new { message = "Subscription soft deleted successfully." });
        }

        [HttpDelete("kyc/document/{documentId}")]
        public async Task<IActionResult> SoftDeleteDocument(Guid documentId)
        {
            var result = await _kycService.SoftDeleteKycDocumentAsync(documentId);
            if (!result)
                return NotFound(new { message = "Document not found or already deleted." });

            return Ok(new { message = "Document soft deleted successfully." });
        }

        [HttpDelete("virtual-number/{dealerId}")]
        public async Task<IActionResult> DeleteVirtualNumber(Guid dealerId)
        {
            var result = await _virtualNumberService.DeleteVirtualNumberAsync(dealerId);
            if (!result)
                return NotFound(new { message = "Virtual number not found or already deleted." });

            return Ok(new { message = "Virtual number deleted successfully." });
        }

        [HttpPatch("business-info/restore/{dealerId}")]
        public async Task<IActionResult> RestoreDealer(Guid dealerId)
        {
            var result = await _dealerService.RestoreDealerAsync(dealerId);
            if (!result)
                return NotFound(new { message = "Dealer not found or not deleted." });

            return Ok(new { message = "Dealer restored successfully." });
        }

        [HttpPatch("user/restore/{userId}")]
        public async Task<IActionResult> RestoreUser(Guid userId)
        {
            var result = await _userDealerService.RestoreUserAsync(userId);
            if (!result)
                return NotFound(new { message = "User not found or not deleted." });

            return Ok(new { message = "User restored successfully." });
        }

        [HttpPatch("subscription/restore/{subscriptionId}")]
        public async Task<IActionResult> RestoreSubscription(Guid subscriptionId)
        {
            var result = await _dealerSubscriptionService.RestoreAsync(subscriptionId);
            if (!result)
                return NotFound(new { message = "Subscription not found or not deleted." });

            return Ok(new { message = "Subscription restored successfully." });
        }

        [HttpPatch("kyc/document/restore/{documentId}")]
        public async Task<IActionResult> RestoreKycDocument(Guid documentId)
        {
            var result = await _kycService.RestoreKycDocumentAsync(documentId);
            if (!result)
                return NotFound(new { message = "Document not found or not deleted." });

            return Ok(new { message = "Document restored successfully." });
        }

        [HttpPatch("virtual-number/restore/{dealerId}")]
        public async Task<IActionResult> RestoreVirtualNumber(Guid dealerId)
        {
            var result = await _virtualNumberService.RestoreVirtualNumberAsync(dealerId);
            if (!result)
                return NotFound(new { message = "Virtual number not found or not deleted." });

            return Ok(new { message = "Virtual number restored successfully." });
        }
    }
}
