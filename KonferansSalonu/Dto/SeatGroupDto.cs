namespace KonferansSalonu.Dto
{
    public class SeatGroupDto
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public int SectionId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }

        public ICollection<DesignItem> Seats { get; set; }=new List<DesignItem>();

    }
}
