using KonferansSalonu.Dto;
using KonferansSalonu.Models;
namespace KonferansSalonu.Services
{
    public interface ISeatGroupService
    {
        Task<bool> CreateSeatGroup(SeatGroupDto seatGroup);

        Task<bool> SaveSeatGroupDesign(int sectionId, List<DesignItem> designItem, List<SeatGroupDto> seatGroupDto);
    }
}
