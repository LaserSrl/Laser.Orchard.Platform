namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public interface IGAProductVM {
        int PartId { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        string Brand { get; set; }
        string Category { get; set; }
        string Variant { get; set; }
        decimal Price { get; set; }
        int Quantity { get; set; }
        string Coupon { get; set; }
        int Position { get; set; }
        string ListName { get; set; }
    }
}