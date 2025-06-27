using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mylittle_project.Controllers
{
    [ApiController]
    [Route("api/dealer")]
    public class DealerController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IUserDealerService _userDealerService;
        private readonly IVirtualNumberService _virtualNumberService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IKycService _kycService;

        public DealerController(
            IBusinessService businessService,
            IUserDealerService userDealerService,
            IVirtualNumberService virtualNumberService,
            ISubscriptionService subscriptionService,
            IKycService kycService)
        {
            _businessService = businessService;
            _userDealerService = userDealerService;
            _virtualNumberService = virtualNumberService;
            _subscriptionService = subscriptionService;
            _kycService = kycService;
        }

        // ──────────────── BUSINESS INFO ────────────────
        [HttpPost("business")]
        public async Task<IActionResult> CreateBusinessInfo([FromBody] DealerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _businessService.CreateBusinessInfoAsync(dto);
            return Ok(new { Message = "Business Info created successfully", BusinessId = id });
        }

        // ──────────────── SUB-USERS ────────────────
        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody] UserDealerDto dto)
        {
            var userId = await _userDealerService.AddUserAsync(dto);
            return Ok(new { UserId = userId });
        }

        [HttpPost("user/batch")]
        public async Task<IActionResult> AddMultipleUsers([FromBody] List<UserDealerDto> users)
        {
            if (users == null || users.Count == 0)
                return BadRequest("User list is empty.");

            var createdUserIds = new List<Guid>();

            foreach (var user in users)
            {
                if (user.BusinessId == Guid.Empty)
                    continue;

                var id = await _userDealerService.AddUserAsync(user);
                createdUserIds.Add(id);
            }

            return Ok(new
            {
                Message = "Users added successfully.",
                UserIds = createdUserIds
            });
        }

        [HttpGet("user/{businessId}")]
        public async Task<IActionResult> GetUsers(Guid businessId)
        {
            var users = await _userDealerService.GetUsersByDealerAsync(businessId);
            return Ok(users);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userDealerService.GetAllUsersAsync();
            return Ok(users);
        }

        // ✅ PAGINATED USERS
        [HttpGet("user/paginated")]
        public async Task<IActionResult> GetPaginatedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userDealerService.GetPaginatedUsersAsync(page, pageSize);
            return Ok(users);
        }

        // ──────────────── VIRTUAL NUMBER ────────────────
        [HttpGet("virtual-number/get/{businessId}")]
        public async Task<IActionResult> GetVirtualNumber(Guid businessId)
        {
            var number = await _virtualNumberService.GetAssignedNumberAsync(businessId);
            return Ok(new { virtualNumber = number });
        }

        // ──────────────── SUBSCRIPTION ────────────────
        [HttpPost("subscription")]
        public async Task<IActionResult> AssignSubscription([FromBody] SubscriptionDealerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _subscriptionService.AssignSubscriptionAsync(dto);
                return Ok(new { Message = "Subscription assigned successfully", SubscriptionId = id });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Error = ex.Message });
            }
        }

        // ──────────────── KYC ────────────────
        [HttpPost("kyc/request")]
        public async Task<IActionResult> AddDocumentRequest([FromBody] KycDocumentRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            await _kycService.AddDocumentRequestAsync(dto);
            return Ok("Document request added successfully.");
        }

        [HttpPost("kyc/upload")]
        public async Task<IActionResult> UploadKycDocument([FromForm] KycDocumentUploadDto dto)
        {
            if (dto.File == null)
                return BadRequest("File is required.");

            var filePath = await _kycService.UploadDocumentAsync(dto);
            return Ok(new { message = "Document uploaded successfully.", filePath });
        }

        [HttpGet("kyc/requested/{businessId}")]
        public async Task<IActionResult> GetRequestedDocuments(Guid businessId)
        {
            var docs = await _kycService.GetRequestedDocumentsAsync(businessId);
            return Ok(docs);
        }
    }
}
