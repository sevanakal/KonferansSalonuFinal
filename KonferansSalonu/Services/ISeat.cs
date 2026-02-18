using KonferansSalonu.Models;
using KonferansSalonu.Dto;
namespace KonferansSalonu.Services
{
    public class ISeat : ISeatService
    {
        private readonly KonferencedbContext _context;

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
                SeatGroupId = seat.SeatGroupId,
                Type = seat.Type,
                X = seat.X,
                Y = seat.Y,
                Label = seat.Label,
                DefaultWidth = seat.DefaultWidth,
                DefaultHeight = seat.DefaultHeight,
                Width = seat.Width,
                Height = seat.Height,
                ScalePercentage = seat.ScalePercentage,
                IsResize = seat.IsResize,
                IsSelected = seat.IsSelected,
                Rotation = seat.Rotation,
                Color = seat.Color
            };
        }
    }
}
