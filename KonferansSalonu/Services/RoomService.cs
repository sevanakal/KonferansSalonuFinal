using KonferansSalonu.Models;
using Microsoft.EntityFrameworkCore;
namespace KonferansSalonu.Services
{
    public class RoomService : IRoomService
    {
        private readonly ConferencedbContext _context;

        public RoomService(ConferencedbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllRoomsAsync() {
            //return await _context.Rooms.Include(x => x.Sections).Where(x=> !x.Isdisabled).AsNoTracking().ToListAsync();
            return await _context.Rooms.Include(x => x.Sections).ThenInclude(s => s.Seatgroups).ThenInclude(sg => sg.Seats).Where(x => !x.Isdisabled).AsNoTracking().AsSplitQuery().ToListAsync();
        }

        public async Task<bool> CreateRoomAsync(Room _room)
        {
            try
            {
                _context.Rooms.Add(_room);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;                
            }
        }

        public async Task<bool> DeleteRoomAsync(int id) 
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    return false;
                }
                room.Isdisabled = true;
                room.Deletedat = DateTime.Now;
                _context.Rooms.Update(room);
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
