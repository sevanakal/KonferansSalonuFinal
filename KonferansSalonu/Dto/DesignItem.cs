namespace KonferansSalonu.Dto
{
    public class DesignItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SeatGroupId { get; set; } = null;
        public string Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Label { get; set; }
        public int DefaultWidth { get; set; } = 40;
        public int DefaultHeight { get; set; } = 40;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int ScalePercentage { get; set; } = 0;
        public bool IsResize { get; set; } = false;
        public bool IsSelected { get; set; }
        public double Rotation { get; set; } = 0;

        public string Color { get; set; }

    }
}
