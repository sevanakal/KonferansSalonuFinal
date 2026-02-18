using KonferansSalonu.Models;
namespace KonferansSalonu.Services
{
    public interface ISectionService
    {
        Task<bool> CreateSectionAsync(Section section);
        Task<List<Section>> GetAllSectionAsync(int roomid);
        Task<bool> DeleteSectionAsync(int id);
    }
}
