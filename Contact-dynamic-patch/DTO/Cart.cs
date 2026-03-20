namespace ContactApi.DTO
{
    public abstract class BaseItem
    {
        public string Id { get; set; } = default!;
    }


    // Base contract shared by all cart items
    public class CartItemDto : BaseItem
    {
        public string Id { get; set; } = default!;
        public int Position { get; set; }             // Controls display/order
        public string Type { get; set; } = default!;  // "product", "text", or "subtotal"
    }


    // ---- Product Item ----
    public class ProductItemDto : CartItemDto
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }

    // ---- Text Item ----
    public class TextItemDto : CartItemDto
    {
        public string Message { get; set; } = default!;
    }

    // ---- Subtotal Item ----
    public class SubtotalItemDto : CartItemDto
    {
        public string Label { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
    }
}