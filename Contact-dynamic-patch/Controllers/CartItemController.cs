using ContactApi.DTO;
using ContactApi.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ContactApi.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        [HttpGet("items")]
        public ActionResult<IEnumerable<CartItemDto>> GetCartItems()
        {
            var items = new List<CartItemDto>
            {
                new ProductItemDto
                {
                    Id = "prod-001",
                    Position = 1,
                    Type = "product",
                    Name = "Wireless Mouse",
                    Price = 29.99m,
                    Quantity = 2,
                    ImageUrl = "https://example.com/images/mouse.jpg"
                },
                new TextItemDto
                {
                    Id = "text-001",
                    Position = 2,
                    Type = "text",
                    Message = "Free shipping on orders over $50!"
                },
                new SubtotalItemDto
                {
                    Id = "subtotal-001",
                    Position = 3,
                    Type = "subtotal",
                    Label = "Subtotal",
                    Amount = 59.98m,
                    Currency = "USD"
                }
            };

            return Ok(items);
        }
    }
}