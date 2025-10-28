using API.ChatVisitasFeedback.Visitas.DTOs;
using API.Models;

namespace API.ChatVisitasFeedback.Visitas.Interfaces
{
    public interface IVisitService
    {
        Visit Request(int userId, int propertyId, DateTime scheduledDate);

        Visit Confirm(int visitId, int userId);

        Visit Cancel(int visitId, int userId);

        Visit Reject(int visitId, int userId);

        Visit Accept(int visitId, int userId);

        IEnumerable<VisitListDTO> List(int userId);

        Visit Get(int chatId);
    }
}
