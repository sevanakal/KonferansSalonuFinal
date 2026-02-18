namespace KonferansSalonu.Services
{
    public interface IClientUiService
    {
        Task<bool> ConfirmDelete(string message);

        Task ShowSuccess(string message);
        Task ShowError(string message);
    }
}
