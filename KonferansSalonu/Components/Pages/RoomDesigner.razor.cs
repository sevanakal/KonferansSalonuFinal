using KonferansSalonu.Services;
using KonferansSalonu.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Transactions;

namespace KonferansSalonu.Components.Pages
{
    public partial class RoomDesigner
    {
        [Inject] public IRoomService RoomService { get; set; }
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
                        SelectedDesignItems.ForEach(x => x.Color = null);
                        SeatGroupColor = "";
                    }
                    SelectedDesignItems = new List<DesignItem>();
                    DesignItems.ForEach(x => x.IsSelected = false);
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
            if (SelectedItem != null)
            {
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

        void OnColorChange() { 
            foreach (var item in SelectedDesignItems)
            {
                item.Color = NewSeatGroup.Color;
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
                if (DesignItems.Any(x => x.SeatGroupId == Guid.Empty))
                {
                    await ClientUiService.ShowError("Gruplandırması yapılmayan yerler var.");
                }
                else
                {
                    // Tüm kontrolleri geçtik, kullanıcıya tasarım verilerini kaydetmek isteyip istemediğini soralım
                    if (await ClientUiService.ConfirmDelete("Tasarım verilerini kaydetmek istiyor musunuz"))
                    {

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
                SeatGroupDto AddadSeatGroup = new SeatGroupDto();
                AddadSeatGroup.Name = NewSeatGroup.Name;
                AddadSeatGroup.Color = NewSeatGroup.Color;
                DesignItem addedItem;
                foreach (var item in SelectedDesignItems)
                {
                    addedItem = new DesignItem();
                    addedItem = item;
                    AddadSeatGroup.Seats.Add(addedItem);
                }
                addedItem = new DesignItem();
                SeatGroups.Add(AddadSeatGroup);
                SelectedDesignItems.ForEach(x => x.SeatGroupId = NewSeatGroup.id);
                SelectedDesignItems.ForEach(x => x.Color = NewSeatGroup.Color);
                SelectedDesignItems.Clear();
                DesignItems.ForEach(x => x.IsSelected = false);
                NewSeatGroup = new SeatGroupDto();
            }
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

        // ViewModel (Geçici Model)
        


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
