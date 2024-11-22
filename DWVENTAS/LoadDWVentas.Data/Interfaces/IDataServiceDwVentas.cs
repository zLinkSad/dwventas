using LoadDWVentas.Data.Result;

namespace LoadDWVentas.Data.Interfaces
{
    public interface IDataServiceDwVentas
    {
        Task<OperactionResult> LoadDHW();
    }
}
