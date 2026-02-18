using CurrieTechnologies.Razor.SweetAlert2;
namespace KonferansSalonu.Services
{
    public class ClientUiService : IClientUiService
    {
        private readonly SweetAlertService _swal;

        public ClientUiService(SweetAlertService swal)
        {
            _swal = swal;
        }

        public async Task<bool> ConfirmDelete(string message) 
        {
            var result = await _swal.FireAsync(new SweetAlertOptions
            {
                Title="Emin Misiniz?",
                Text=message,
                Icon=SweetAlertIcon.Warning,
                ShowCancelButton=true,
                ConfirmButtonText="Evet",
                CancelButtonText="Hayır",
                ConfirmButtonColor="#3085d6",
                CancelButtonColor= "#d33"
            });

            return !string.IsNullOrEmpty(result.Value);
        }

        public async Task ShowSuccess(string message)
        {
            await _swal.FireAsync(new SweetAlertOptions
            {
                Title = "İşlem Başarılı",
                Text = message,
                Icon= SweetAlertIcon.Success,
                Timer=3000,
                ShowConfirmButton= false
            });
        }

        public async Task ShowError(string message) 
        { 
            await _swal.FireAsync(new SweetAlertOptions
            {
                Title = "Hata",
                Text = message,
                Icon = SweetAlertIcon.Error,
                Timer = 5000,
                ShowConfirmButton = false
            });
        }
    }
}
