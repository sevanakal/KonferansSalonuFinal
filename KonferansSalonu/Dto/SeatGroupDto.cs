namespace KonferansSalonu.Dto
{
    public class SeatGroupDto
    {
        public int id { get; set; } = 0;
        public int SectionId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }

        public ICollection<DesignItem> Seats { get; set; }=new List<DesignItem>();

    }
}
