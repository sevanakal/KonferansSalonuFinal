using KonferansSalonu.Models;
using KonferansSalonu.Dto;
namespace KonferansSalonu.Services
{
    public class ISeat : ISeatService
    {
        private readonly ConferencedbContext _context;

        public ISeat(ConferencedbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateSeatAsync(DesignItem seat) 
        { 
            if (seat == null)
            {
                return false;
            }

            var newSeat = new Seat
            {
                Type = seat.Type,
                X = seat.X,
                Y = seat.Y,
                Label = seat.Label,
                Defaultwidth = seat.DefaultWidth,
                Defaultheight = seat.DefaultHeight,
                Width = seat.Width,
                Height = seat.Height,
                Scalepercentage = seat.ScalePercentage,
                Isresize = seat.IsResize ? 1 : 0,
                Rotation = seat.Rotation
            };
            _context.Seats.Add(newSeat);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
