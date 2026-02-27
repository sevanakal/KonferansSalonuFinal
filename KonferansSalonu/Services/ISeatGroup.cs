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
                        newGroup.Seats.Add(MaptToSeatEntity(seatItems, sectionId));
                    }
                    newSeatGroups.Add(newGroup);

                    await _context.Seatgroups.AddRangeAsync(newSeatGroups);
                    

                    
                }
                var nonSeatItems = designItem.Where(d => d.SeatGroupId == 0).ToList();
                foreach (var item in nonSeatItems)
                {
                    _context.Seats.Add(MaptToSeatEntity(item, sectionId));
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Veritabanı kayıt işlemi sırasında kritik bir hata oluştu ve işlemler geri alındı: " + ex.Message);
            }
            return false;
        }

        public async Task<SeatGroupsAndObject> ListSeatGroupDesign(int sectionId)
        {
            var seatGroups = await _context.Seatgroups.Include(g => g.Seats).Where(g => g.Sectionid == sectionId).ToListAsync();

            SeatGroupsAndObject _SeatGroupAndObjects=new SeatGroupsAndObject();
            List<SeatGroupDto> seatGroupDtosList = new List<SeatGroupDto>();
            foreach (var group in seatGroups)
            {
                var seatGroupDto = MapToSeatGroupDto(group);
                List<DesignItem> designItems = new List<DesignItem>();
                foreach (var seat in group.Seats)
                {
                    designItems.Add(MapToDesignItem(seat, group.Color));
                }
                seatGroupDto.Seats = designItems;
                seatGroupDtosList.Add(seatGroupDto);
            }

            
            _SeatGroupAndObjects.SeatGrpups = seatGroupDtosList;

            var _objects = await _context.Seats.Where(s => s.Sectionid == sectionId && s.Seatgroupid == null).ToListAsync();
            foreach (var obj in _objects) {
                _SeatGroupAndObjects.Objects.Add(MapToDesignItem(obj, ""));
            }

            return _SeatGroupAndObjects;
        }

        private SeatGroupDto MapToSeatGroupDto(Seatgroup seatGroup)
        {
            return new SeatGroupDto
            {
                id= seatGroup.Id,
                SectionId = seatGroup.Sectionid,
                Name = seatGroup.Name,
                Color = seatGroup.Color
            };
        }

        private DesignItem MapToDesignItem(Seat seat, string color)
        {
            return new DesignItem
            {
                SeatGroupId = seat.Seatgroupid,
                Label = seat.Label,
                Type = seat.Type,
                X = seat.X,
                Y = seat.Y,
                Rotation = seat.Rotation,
                DefaultWidth = seat.Defaultheight ?? 0,
                DefaultHeight = seat.Defaultheight ?? 0,
                Width = seat.Width ?? 0,
                Height = seat.Height ?? 0,
                ScalePercentage = seat.Scalepercentage ?? 0,
                IsResize = seat.Isresize == 1,
                Color = color,
                PreColor = color,
            };
        }

        private Seat MaptToSeatEntity(DesignItem designItem, int sectionID)
        {
            return new Seat
            {
                Sectionid = sectionID,
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
