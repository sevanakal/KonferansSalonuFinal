using KonferansSalonu.Services;
using KonferansSalonu.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Serialization;

namespace KonferansSalonu.Components.Pages
{
    public partial class RoomDesigner
    {
        [Inject] public IRoomService RoomService { get; set; }
        [Inject] public ISeatGroupService SeatGroupService { get; set; }
        [Inject] public NavigationManager NavManager { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public IClientUiService ClientUiService { get; set; }

        [Parameter] public int SectionId { get; set; }

        private IJSObjectReference? _module;

        private double selectionStartX, selectionStartY; // Seçim kutusunun başladığı nokta
        private BoundingRect? rect;

        // Sahnedeki Tüm Eşyalar
        List<DesignItem> DesignItems = new List<DesignItem>();
        List<DesignItem> SelectedDesignItems = new List<DesignItem>();

        // Şu an sürüklenen veya seçilen eşya
        DesignItem? SelectedItem;
        DesignItem? _clipboardItem;


        // Sürükleme durumu
        bool isDragging = false;
        double startX, startY; // Farenin ilk bastığı yer
        double itemStartX, itemStartY; // Eşyanın ilk konumu

        double PanX = 0, PanY = 0; // Pan için
        bool IsPanning = false;
        double StartPanX, StartPanY;

        int gridSize = 20; // Izgara boyutu

        bool dragPanControl = true; //Sürükleme ve Pan işlem kontrolü

        string SeatGroupColor = "#000000"; // Yeni grup için varsayılan renk

        //Selectionbox
        SelectionBox selectionBox = new SelectionBox();

        //Eklene Seat Grupları
        public List<SeatGroupDto> SeatGroups = new List<SeatGroupDto>();

        public SeatGroupDto NewSeatGroup = new SeatGroupDto ();

        bool hasSeat = false; //Nesne eğer daha önce başka bir yerde gruba atandıysa

        protected override async Task OnInitializedAsync()
        {
            await LoadDesign();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/RoomDesigner.razor.js");
            }
        }

        // 1. EKLEME
        void AddItem(string type)
        {
            var newItem = new DesignItem
            {
                Type = type,
                Label = GetNextLabel(type),
                X = 100, // Varsayılan canvas ortasına at
                Y = 100
            };
            switch (newItem.Type)
            {
                case "table":
                    newItem.DefaultWidth = 60;
                    newItem.DefaultHeight = 60;
                    newItem.Width = 60;
                    newItem.Height = 60;
                    newItem.IsResize = true;
                    break;
                case "rectangletable":
                    newItem.DefaultWidth = 200;
                    newItem.DefaultHeight = 100;
                    newItem.Width = 200;
                    newItem.Height = 100;
                    newItem.IsResize = true;
                    break;
            }

            DesignItems.Add(newItem);
            SelectedItem = newItem; // Ekleyince otomatik seç
        }

        // 2. SEÇME VE SÜRÜKLEMEYİ BAŞLATMA
        void StartDrag(DesignItem item, MouseEventArgs e)
        {
            if (dragPanControl)
            {
                // Önce diğerlerinin seçimini kaldır
                DesignItems.ForEach(x => x.IsSelected = false);

                item.IsSelected = true;
                SelectedItem = item;

                isDragging = true;

                // Farenin neresinden tuttuğumuzu hesaplayalım
                startX = e.ClientX;
                startY = e.ClientY;

                itemStartX = item.X;
                itemStartY = item.Y;
            }
            else
            {
                if (e.CtrlKey)
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                        SelectedItem = null;
                        if(SelectedDesignItems.Any(x=> x.Id == item.Id))
                        {
                            var removeItem = SelectedDesignItems.FirstOrDefault(x => x.Id == item.Id);
                            if (removeItem != null)
                                SelectedDesignItems.Remove(removeItem);
                        }
                    }
                    else
                    {
                        item.IsSelected = true;
                        SelectedItem = item;
                        if (!SelectedDesignItems.Any(x => x.Id == item.Id))
                            SelectedDesignItems.Add(item);
                    }
                }
                else
                {
                    SelectedDesignItems = new List<DesignItem>();
                    DesignItems.ForEach(x => x.IsSelected = false);

                    item.IsSelected = true;
                    SelectedItem = item;
                    
                }

            }

        }

        //Yerleşim ve Pan işlemi arasında geçiş yaparken sürüklemenin birbirine karışmaması için kontrol ekleyelim
        void DragPanToggle(ChangeEventArgs e)
        {
            dragPanControl = (bool)(e.Value ?? false);
        }

        // 3. FARE HAREKETİ (CANVAS ÜZERİNDE)
        void OnMouseMove(MouseEventArgs e)
        {
            if (dragPanControl)
            {
                if (IsPanning)
                {
                    PanX = e.ClientX - StartPanX;
                    PanY = e.ClientY - StartPanY;
                }
                else if (isDragging && SelectedItem != null)
                {
                    // Ne kadar hareket ettik?
                    double deltaX = e.ClientX - startX;
                    double deltaY = e.ClientY - startY;

                    double RawX = itemStartX + deltaX;
                    double RawY = itemStartY + deltaY;

                    // Yeni konumu güncelle
                    SelectedItem.X = Math.Round(RawX / gridSize) * gridSize;
                    SelectedItem.Y = Math.Round(RawY / gridSize) * gridSize;

                    // Grid'e yapışma (Snap to Grid) özelliği - Opsiyonel
                    // Her 20px'e yuvarlar
                    // SelectedItem.X = Math.Round((itemStartX + deltaX) / 20) * 20;
                    // SelectedItem.Y = Math.Round((itemStartY + deltaY) / 20) * 20;
                }
            }
            else
            {
                if (selectionBox.IsActive)
                {
                    double mouseInCanvasX = e.ClientX - rect.X;
                    double mouseInCanvasY = e.ClientY - rect.Y;

                    double currentWorldX = mouseInCanvasX - PanX;
                    double currentWorldY = mouseInCanvasY - PanY;

                    selectionBox.X= Math.Min(selectionStartX, currentWorldX);
                    selectionBox.Y = Math.Min(selectionStartY, currentWorldY);

                    selectionBox.Width = Math.Abs(currentWorldX - selectionStartX);
                    selectionBox.Height = Math.Abs(currentWorldY - selectionStartY);

                    bool hasChanged = false;
                    if (!e.CtrlKey)
                    {

                    }
                    foreach (var item in DesignItems)
                    {
                        
                        bool isHit = CheckIntersection(item, selectionBox);
                        if (isHit)
                        {
                            if (!SelectedDesignItems.Any(x => x.Id == item.Id))
                                SelectedDesignItems.Add(item);
                            item.IsSelected = isHit;
                            hasChanged = true;
                        }
                        else
                        {
                            if (SelectedDesignItems.Any(x => x.Id == item.Id)) 
                            { 
                                var removeItem = SelectedDesignItems.FirstOrDefault(x => x.Id == item.Id);
                                if (removeItem != null)
                                {
                                    SelectedDesignItems.Remove(removeItem);
                                    item.IsSelected = false;
                                    hasChanged = true;
                                }
                            }
                        }

                    }
                    if (hasChanged)
                    {
                        StateHasChanged();
                    }
                }
            }

        }

        async Task StartPan(MouseEventArgs e)
        {
            if (e.Button == 0) // Sol tuş ile pan yapalım
            {
                SelectedItem = null;
                if (dragPanControl)
                {
                    IsPanning = true;
                    StartPanX = e.ClientX - PanX;
                    StartPanY = e.ClientY - PanY;
                }
                else
                {
                    if(SelectedDesignItems.Count() > 0)
                    {
                        foreach (var item in SelectedDesignItems.Where(x => x.SeatGroupId == Guid.Empty).ToList())
                        {
                            item.Color = "";
                        }
                        
                        foreach (var item in SelectedDesignItems)
                        {
                            if (item.Color != item.PreColor)
                            {
                                item.Color = item.PreColor;
                            }
                        }
                        NewSeatGroup.Name = "";
                        NewSeatGroup.Color = "";
                        SelectedDesignItems = new List<DesignItem>();
                        DesignItems.ForEach(x => x.IsSelected = false);
                    }
                    
                    rect = await JSRuntime.InvokeAsync<BoundingRect>("getElementBoundingClient", "designer-canvas");
                    if (rect == null) return;

                    double mouseInCanvasX = e.ClientX - rect.X;
                    double mouseInCanvasY = e.ClientY - rect.Y;

                    double worldX = mouseInCanvasX - PanX;
                    double worldY = mouseInCanvasY - PanY;

                    selectionStartX = worldX;
                    selectionStartY = worldY;

                    selectionBox.X = worldX;
                    selectionBox.Y = worldY;

                    selectionBox.IsActive = true;

                }

            }
            
        }

        void StopPan()
        {
            IsPanning = false;
            isDragging = false; // Pan yaparken sürükleme olmasın
            selectionBox = new SelectionBox();
        }

        // 4. BIRAKMA
        void StopDrag()
        {
            isDragging = false;
        }

        void DeleteItem()
        {
            // Çoklu seçim varsaepsini sil
            if (SelectedDesignItems.Any())
            {

                foreach (var item in SelectedDesignItems)
                {
                    // 1. Eğer bir gruba aitse, direkt o grubu bul
                    if (item.SeatGroupId != Guid.Empty)
                    {
                        var targetGroup = SeatGroups.FirstOrDefault(g => g.id == item.SeatGroupId);
                        if (targetGroup != null)
                        {
                            // 2. O grubun içinden bu koltuğu uçur
                            var seatToRemove = targetGroup.Seats.FirstOrDefault(s => s.Id == item.Id);
                            if (seatToRemove != null)
                            {
                                targetGroup.Seats.Remove(seatToRemove);
                            }
                        }
                    }

                    // 3. Sahneden (Canvas) uçur
                    DesignItems.Remove(item);
                }
                SelectedDesignItems.Clear(); // Seçim listesini boşalt
                SelectedItem = null;
            }
            // Sadece tekil tıklanmış bir şey varsa
            else if (SelectedItem != null)
            {
                foreach (var _seatGroup in SeatGroups)
                {
                    var _seat = _seatGroup.Seats.FirstOrDefault(x => x.Id == SelectedItem.Id);
                    if (_seat != null)
                    {
                        _seatGroup.Seats.Remove(_seat);
                    }
                }
                DesignItems.Remove(SelectedItem);
                SelectedItem = null;
            }
        }

        void ScalePercentage(ChangeEventArgs e)
        {
            if (SelectedItem != null)
            {
                if (int.TryParse(e.Value?.ToString(), out int ratio))
                {
                    SelectedItem.ScalePercentage = ratio;
                    double carpan = 1 + (ratio / 100.0);
                    SelectedItem.Width = Convert.ToInt32(SelectedItem.DefaultWidth * carpan);
                    SelectedItem.Height = Convert.ToInt32(SelectedItem.DefaultHeight * carpan);
                }
            }
        }

        void HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.CtrlKey && e.Key.ToLower() == "c")
            {
                if (SelectedItem != null)
                {
                    _clipboardItem = new DesignItem
                    {
                        Type = SelectedItem.Type,
                        Label = GetNextLabel(SelectedItem.Type),
                        Rotation = SelectedItem.Rotation,
                        IsResize = SelectedItem.IsResize,
                        DefaultWidth = SelectedItem.DefaultWidth,
                        DefaultHeight = SelectedItem.DefaultHeight,
                        Width = SelectedItem.Width,
                        Height = SelectedItem.Height,
                        ScalePercentage = SelectedItem.ScalePercentage
                    };
                }
            }

            if (e.CtrlKey && e.Key.ToLower() == "v")
            {
                if (_clipboardItem != null)
                {

                    var newItem = new DesignItem
                    {
                        Type = _clipboardItem.Type,
                        Label = _clipboardItem.Label,
                        X = 150,
                        Y = 150,
                        Rotation = _clipboardItem.Rotation,
                        IsSelected = true,
                        IsResize = _clipboardItem.IsResize,
                        DefaultWidth = _clipboardItem.DefaultWidth,
                        DefaultHeight = _clipboardItem.DefaultHeight,
                        Width = _clipboardItem.Width,
                        Height = _clipboardItem.Height,
                        ScalePercentage = _clipboardItem.ScalePercentage

                    };
                    if (SelectedItem != null)
                    {
                        newItem.Label = GetNextLabel(newItem.Type);
                        newItem.X = SelectedItem.X + 20;
                        newItem.Y = SelectedItem.Y + 20;
                        SelectedItem.IsSelected = false;
                    }
                    DesignItems.Add(newItem);
                    SelectedItem = newItem;


                }
            }

            

            if (e.Key == "Delete" || e.Key == "Backspace")
            {
                DeleteItem();
            }



        }

        private bool CheckIntersection(DesignItem item, SelectionBox box)
        {
            //Eşyanın genişlik ve yükseklik değerleri
            double w = item.Width > 0 ? item.Width : item.DefaultWidth;
            double h = item.Height > 0 ? item.Height : item.DefaultHeight;

            //Eşyanın sınırları
            double itemLeft = item.X;
            double itemRight = item.X + w;
            double itemTop = item.Y;
            double itemBottom = item.Y + h;

            //Seçim kutusunun sınırları
            double boxLeft = box.X;
            double boxRight = box.X + box.Width;
            double boxTop = box.Y;
            double boxBottom = box.Y + box.Height;

            //Mantık eğer biri kesişirse
            return !(boxLeft > itemRight || boxRight < itemLeft || boxTop > itemBottom || boxBottom < itemTop);
        }

        string GetNextLabel(string type)
        {
            int nextNumber = DesignItems.Count(x => x.Type == type) + 1;
            return type switch
            {
                "armchair" => $"K-{nextNumber}",
                "chair" => $"S-{nextNumber}",
                "table" => $"M-{nextNumber}",
                "rectangletable" => $"DM-{nextNumber}",
                "stage" => $"SAHNE-{nextNumber}",
                _ => "NULL"
            };
        }

        void GoBack() => NavManager.NavigateTo("/rooms");

        async Task OnColorChange() {
            // Seçili eşyalardan herhangi birinin SeatGroupId'si atanmış mı? (Guid.Empty değilse atanmıştır)
            //bool isAlreadyGrouped = SelectedDesignItems.Any(x => x.SeatGroupId != Guid.Empty);

            //if (isAlreadyGrouped)
            //{
            //    await ClientUiService.ShowError("Dikkat!!! Seçtiğiniz nesnelerden biri veya birkaçı daha önce başka grupta tanımlanmış.");
            //    return; // İşlemi kes
           // }

            // Sorun yoksa rengi daya!
            foreach (var item in SelectedDesignItems)
            {
                item.Color = NewSeatGroup.Color;
            }
        }
        async Task OnColorChangeCreatadGroup()
        {
            // Seçili eşyalardan herhangi birinin SeatGroupId'si atanmış mı? (Guid.Empty değilse atanmıştır)
            //bool isAlreadyGrouped = SelectedDesignItems.Any(x => x.SeatGroupId != Guid.Empty);

            //if (isAlreadyGrouped)
            //{
            //    await ClientUiService.ShowError("Dikkat!!! Seçtiğiniz nesnelerden biri veya birkaçı daha önce başka grupta tanımlanmış.");
            //    return; // İşlemi kes
            // }

            // Sorun yoksa rengi daya!
            var color = SeatGroups.FirstOrDefault(x => x.Name == NewSeatGroup.Name).Color;
            NewSeatGroup.Color = color;
            foreach (var item in SelectedDesignItems)
            {
                item.Color = color;
            }
        }

        async Task AddSeats() {
            // Öncelikle tasarım verisi var mı kontrol edelim
            if (DesignItems.Count() == 0)
            {
                await ClientUiService.ShowError("Henüz hiçbir tasarım verisi girmediniz!");
            }
            else
            {
                // Tasarım verilerinde grup ataması yapılmamış yerler var mı kontrol edelim
                if (DesignItems.Where(x=>x.Type== "armchair" || x.Type== "chair").Any(x => x.SeatGroupId == Guid.Empty))
                {
                    await ClientUiService.ShowError("Gruplandırması yapılmayan yerler var.");
                }
                else
                {
                    // Tüm kontrolleri geçtik, kullanıcıya tasarım verilerini kaydetmek isteyip istemediğini soralım
                    if (await ClientUiService.ConfirmDelete("Tasarım verilerini kaydetmek istiyor musunuz"))
                    {
                        if (await SeatGroupService.SaveSeatGroupDesign(SectionId, DesignItems, SeatGroups))
                        {
                            await ClientUiService.ShowSuccess("Tasarım verileri başarıyla kaydedildi!");
                        }
                        else { 
                            await ClientUiService.ShowError("Tasarım verileri kaydedilirken bir hata oluştu!");
                        }
                    }
                }
                
            }
        }

        async Task AddSeatGroupView()
        {
            if (String.IsNullOrEmpty(NewSeatGroup.Name))
            {
                await ClientUiService.ShowError("Grup adı girmediniz!");
            }else if (SelectedDesignItems.Count() == 0)
            {
                await ClientUiService.ShowError("Lütfen önce seçim yapınız!");
            }else if(NewSeatGroup.Color=="Rengini Seç:" || String.IsNullOrEmpty(NewSeatGroup.Color))
            {
                await ClientUiService.ShowError("Grup rengini belirlemediniz!");
            }
            else
            {
                await AddSeatGroupSection(NewSeatGroup.Name, NewSeatGroup.Color);

            }
        }

        async Task AddSeatGroupSection(string name, string color)
        {
            var hasSeatGroup = SeatGroups.FirstOrDefault(x => x.Name == name);

            if (hasSeatGroup != null)
            {
                if (hasSeatGroup.Color != name)
                {
                    await ClientUiService.ShowError("Eklemek istediğiniz grup başka bir renk seçimi ile oluşturulmuş!");
                    return;
                }
            }



            //if (hasSeatGroup == null)
            //{
            SeatGroupDto AddedSeatGroup = new SeatGroupDto();
            AddedSeatGroup.Name = name;
            AddedSeatGroup.Color = color;
            DesignItem addedItem;
            foreach (var item in SelectedDesignItems)
            {
                if (SeatGroups.Count() > 0)
                {
                    foreach (var _group in SeatGroups)
                    {
                        var _seatControl = _group.Seats.FirstOrDefault(x => x.Id == item.Id);
                        if (_seatControl != null)
                        {
                            _seatControl.SeatGroupId = Guid.Empty;
                            _group.Seats.Remove(_seatControl);
                        }
                    }
                }

                item.PreColor = item.Color;
                if (hasSeatGroup == null)
                {
                    item.Color = NewSeatGroup.Color;
                    item.SeatGroupId = AddedSeatGroup.id;
                    AddedSeatGroup.Seats.Add(item); // Zaten referans taşıdığımız için direkt ekliyoruz
                }
                else
                {
                    item.Color = hasSeatGroup.Color;
                    item.SeatGroupId = hasSeatGroup.id;
                    hasSeatGroup.Seats.Add(item);
                }

            }
            addedItem = new DesignItem();
            if (hasSeatGroup == null)
            {
                SeatGroups.Add(AddedSeatGroup);
            }


            DesignItems.ForEach(x => x.IsSelected = false);
            NewSeatGroup = new SeatGroupDto();
            StateHasChanged();

        }

        async Task DeleteSeatGroupView(SeatGroupDto seatGroupDto)
        {
            if (await ClientUiService.ConfirmDelete($"{seatGroupDto.Name} grubunu silmek istediğinize emin misiniz?"))
            {
                
                foreach (var item in seatGroupDto.Seats)
                {
                    var updateSeat = DesignItems.Find(x => x.Id == item.Id);
                    if (updateSeat!=null)
                    {
                        updateSeat.Color = "";
                        updateSeat.SeatGroupId = Guid.Empty;
                    }
                }
                SeatGroups.Remove(seatGroupDto);
            }
        }

        async Task SelectSeatGroup(SeatGroupDto _SeatGroup)
        {
            DesignItems.ForEach(x => x.IsSelected = false);
            DesignItems.Where(x => x.SeatGroupId == _SeatGroup.id).ToList().ForEach(x => x.IsSelected = true);
            var list = DesignItems.Where(x => x.SeatGroupId == _SeatGroup.id).ToList();
            foreach (var item in list)
            {
                SelectedDesignItems.Add(item);
            }
        }

        async Task ExcludeDesignItem()
        {
            if(await ClientUiService.ConfirmDelete("Seçili objeyi gruptan çıkarmak istediğinize emin miziniz?"))
            {
                if (SelectedItem != null)
                {
                    var group = SeatGroups.FirstOrDefault(x => x.id == SelectedItem.SeatGroupId);
                    var seat = group?.Seats.FirstOrDefault(x => x.Id == SelectedItem.Id);
                    if (group != null && seat != null)
                    {
                        group.Seats.Remove(seat);
                        SelectedItem.SeatGroupId = Guid.Empty;
                        SelectedItem.Color = "";
                        SelectedItem.PreColor = "";
                    }
                }
                if (SelectedDesignItems.Count() > 0)
                {
                    
                    foreach (var _seat in SelectedDesignItems)
                    {
                        // Sadece koltuğun ait olduğu grubu bul! Bütün grupları dönme.
                        if (_seat.SeatGroupId != Guid.Empty)
                        {
                            var targetGroup = SeatGroups.FirstOrDefault(g => g.id == _seat.SeatGroupId);
                            if (targetGroup != null)
                            {
                                var removeSeat = targetGroup.Seats.FirstOrDefault(x => x.Id == _seat.Id);
                                if (removeSeat != null)
                                {
                                    targetGroup.Seats.Remove(removeSeat);
                                }
                            }
                        }
                    }
                    SelectedDesignItems.ForEach(x => x.Color = "");
                    SelectedDesignItems.ForEach(x => x.PreColor = "");
                    SelectedDesignItems.ForEach(x => x.IsSelected = false);
                    SelectedDesignItems.Clear();
                }
            }
            
        }

        // ViewModel (Geçici Model)


        public async Task LoadDesign() {
            var SeatGroupList = await SeatGroupService.ListSeatGroupDesign(SectionId);
            if(SeatGroupList.SeatGrpups.Any())
            {
                DesignItems = SeatGroupList.SeatGrpups.SelectMany(g => g.Seats).ToList();
                DesignItems.AddRange(SeatGroupList.Objects);
                foreach (var _SeatGroup in SeatGroupList.SeatGrpups) {
                    SeatGroups.Add(new SeatGroupDto { 
                        Name = _SeatGroup.Name,
                        Color = _SeatGroup.Color
                    });
                }
                StateHasChanged();
            }
        }
        public class SelectionBox
        {
            public double X { get; set; } = 0;
            public double Y { get; set; } = 0;
            public double Width { get; set; } = 0;
            public double Height { get; set; } = 0;
            public bool IsActive { get; set; } = false;
        }

        public class BoundingRect { 
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
    }
}
