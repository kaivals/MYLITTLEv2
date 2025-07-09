using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class OrderUpdateDto
    {
        [Required(ErrorMessage = "Buyer name is required.")]
        [MaxLength(100, ErrorMessage = "Buyer name cannot exceed 100 characters.")]
        public string BuyerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Portal name is required.")]
        public string Portal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dealer name is required.")]
        public string Dealer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment status is required.")]
        public string PaymentStatus { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Order date is required.")]
        public DateTime OrderDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one order item is required.")]
        public List<OrderItemUpdateDto> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
    }
}
