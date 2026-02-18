using KonferansSalonu.Dto;
using KonferansSalonu.Models;
namespace KonferansSalonu.Services
{
    public class ISeatGroup : ISeatGroupService
    {
        private readonly ConferencedbContext _context;

        public ISeatGroup(ConferencedbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateSeatGroup(SeatGroupDto seatgroup)
        {
            if(seatgroup == null)
            {
                return false;
            }
            else
            {
                Seatgroup _seatGroup= new Seatgroup
                {
                    Sectionid = seatgroup.SectionId,
                    Name = seatgroup.Name,
                    Color = seatgroup.Color
                };
                await _context.Seatgroups.AddAsync(_seatGroup);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
