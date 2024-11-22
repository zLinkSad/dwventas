

namespace LoadDWVentas.Data.Result
{
    public class OperactionResult
    {
        public OperactionResult()
        {
            this.Success = true;
        }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
