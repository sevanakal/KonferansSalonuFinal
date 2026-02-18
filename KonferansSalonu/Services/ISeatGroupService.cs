using KonferansSalonu.Dto;
using KonferansSalonu.Models;
namespace KonferansSalonu.Services
{
    public interface ISeatGroupService
    {
        Task<bool> CreateSeatGroup(SeatGroupDto seatGroup);
    }
}
