using KonferansSalonu.Dto;
using KonferansSalonu.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> SaveSeatGroupDesign(int sectionId, List<DesignItem> designItem, List<SeatGroupDto> seatGroupDto) 
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Eski koltuk gruplarını ve koltukları sil
                var oldGroups = await _context.Seatgroups.Include(g => g.Seats).Where(g => g.Sectionid == sectionId).ToListAsync();

                var oldSeats = oldGroups.SelectMany(g => g.Seats).ToList();

                _context.Seats.RemoveRange(oldSeats);
                _context.Seatgroups.RemoveRange(oldGroups);
                await _context.SaveChangesAsync();

                var newSeatGroups = new List<Seatgroup>();
                var newSeats = new List<Seat>();

                foreach (var group in seatGroupDto)
                {
                    var newGroup = new Seatgroup
                    {
                        Sectionid = sectionId,
                        Name = group.Name,
                        Color = group.Color
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }

        private Seat MaptToSeatEntity(DesignItem designItem, int seatGroupId)
        {
            return new Seat
            {
                
                
                
            };
        }
    }
}
