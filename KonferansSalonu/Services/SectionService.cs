using KonferansSalonu.Models;
using Microsoft.EntityFrameworkCore;

namespace KonferansSalonu.Services
{
    public class SectionService : ISectionService
    {
        private readonly ConferencedbContext _context;

        public SectionService(ConferencedbContext context)
        {
            _context = context;
        }


        public async Task<bool> CreateSectionAsync(Section _section)
        {
            try
            {
                _context.Sections.Add(_section);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<List<Section>> GetAllSectionAsync(int roomid)
        {
            return await _context.Sections.Include(s => s.Seatgroups).Where(s => s.Roomid == roomid).ToListAsync();
        }

        public async Task<bool> DeleteSectionAsync(int id) 
        {
            try
            {
                var section = await _context.Sections.FindAsync(id);
                if (section == null)
                {
                    return false;
                }
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
