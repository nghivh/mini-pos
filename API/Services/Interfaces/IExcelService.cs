namespace API.Services.Interfaces
{
    public interface IExcelService
    {
        Task<byte[]> ExportDeliveryNoteAsync(string req_nbr);
    }
}
