using API.ChatVisitasFeedback.Feedbacks.DTOs;
using API.Models;

namespace API.ChatVisitasFeedback.Feedbacks.Interfaces
{
    public interface IFeedbackService
    {
        public Feedback Submit(int userId, int visitId, byte rating, string? message);

        public IEnumerable<FeedbackListDTO> List(int agentId);
    }
}
