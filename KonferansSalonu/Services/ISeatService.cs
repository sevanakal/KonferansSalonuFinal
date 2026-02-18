using KonferansSalonu.Dto;
namespace KonferansSalonu.Services
{
    public interface ISeatService
    {
        Task<bool> CreateSeatAsync(DesignItem seat);
    }
}
