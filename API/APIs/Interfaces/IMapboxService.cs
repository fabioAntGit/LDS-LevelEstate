namespace API.APIs.Interfaces
{
    public interface IMapboxService
    {
        Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address);
    }
}
