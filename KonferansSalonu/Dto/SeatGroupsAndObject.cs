using KonferansSalonu.Models;

namespace KonferansSalonu.Dto
{
    public class SeatGroupsAndObject
    {
        public List<SeatGroupDto> SeatGrpups { get; set; } = new List<SeatGroupDto>();

        public List<DesignItem> Objects { get; set; } = new List<DesignItem>();
    }
}
