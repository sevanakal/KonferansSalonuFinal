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

                _context.ChangeTracker.Clear();

                var newSeatGroups = new List<Seatgroup>();
                var newSeats = new List<Seat>();

                foreach (var group in seatGroupDto)
                {
                    var newGroup = new Seatgroup
                    {
                        Sectionid = sectionId,
                        Name = group.Name,
                        Color = group.Color,
                        Seats = new List<Seat>()
                    };
                    var itemsInGroup = designItem.Where(d => d.SeatGroupId == group.id).ToList();
                    foreach (var seatItems in itemsInGroup) {
                        newGroup.Seats.Add(MaptToSeatEntity(seatItems));
                    }
                    newSeatGroups.Add(newGroup);

                    await _context.Seatgroups.AddRangeAsync(newSeatGroups);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Veritabanı kayıt işlemi sırasında kritik bir hata oluştu ve işlemler geri alındı: " + ex.Message);
            }
            return false;
        }

        private Seat MaptToSeatEntity(DesignItem designItem)
        {
            return new Seat
            {
                Label = designItem.Label,
                Type = designItem.Type,
                X = designItem.X,
                Y = designItem.Y,
                Rotation = designItem.Rotation,
                Defaultwidth = designItem.DefaultWidth,
                Defaultheight = designItem.DefaultHeight,
                Width = designItem.Width,
                Height = designItem.Height,
                Scalepercentage = designItem.ScalePercentage,
                Isresize = designItem.IsResize ? 1 : 0
            };
        }
    }
}
