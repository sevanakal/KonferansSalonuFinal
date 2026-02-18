using KonferansSalonu.Models;
namespace KonferansSalonu.Services
{
    public interface IRoomService
    {
        //Tüm Salonları Getir
        Task<List<Room>> GetAllRoomsAsync();

        //Yeni Salon Ekle
        Task<bool> CreateRoomAsync(Room room);

        //Salon Sil
        Task<bool> DeleteRoomAsync(int id);
    }
}
