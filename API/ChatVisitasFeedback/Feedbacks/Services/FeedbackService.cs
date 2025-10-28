using API.ChatVisitasFeedback.Feedbacks.DTOs;
using API.ChatVisitasFeedback.Feedbacks.Exceptions;
using API.ChatVisitasFeedback.Feedbacks.Interfaces;
using API.ChatVisitasFeedback.Visitas.Exceptions;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.ChatVisitasFeedback.Feedbacks.Services
{
    public class FeedbackService : IFeedbackService
    {

        private readonly LevelEstateContext context;
        private readonly IAgentService agentService;

        public FeedbackService(LevelEstateContext context, IAgentService agentService)
        {
            this.context = context;
            this.agentService = agentService;
        }

        public Feedback Submit(int userId, int visitId, byte rating, string? message)
        {
            var visit = context.Visits
                .Include(v => v.Feedback)
                .FirstOrDefault(v => v.VisitId == visitId);

            if (visit == null)
                throw new VisitDoesntExistException("This visit doesnt exist");

            if (visit.Status != VisitStatus.Confirmed)
                throw new VisitNotConfirmedException("Cannot submit feedback for a visit that is not confirmed");

            if (visit.ClientId != userId)
                throw new FeedbackForbiddenActionException("User is not authorized to submit feedback for this visit");

            if (visit.Feedback != null)
                throw new FeedbackAlreadyExistsException("A feedback for this visit already exists");

            if(rating > 5 || rating < 1)
                throw new FeedbackInvalidRatingException("Rating must be between 1 and 5");

            if(!string.IsNullOrWhiteSpace(message) && message.Length > 150)
                throw new FeedbackMessageTooLongException("feedback message cannot exceed 150 characters");

            string? messageToStore = string.IsNullOrWhiteSpace(message) ? null : message;
            
            var feedback = new Feedback
            {
                VisitId = visitId,
                Rate = rating,
                Message = messageToStore,
                CreatedAt = DateTime.UtcNow
            };

            context.Add(feedback);
            visit.Feedback = feedback;
            context.SaveChanges();
            AwardPointsToAgent(visit.AgentId, rating);

            return feedback;
        }

        public IEnumerable<FeedbackListDTO> List(int agentId)
        {
            var agentExists = context.Agents.Any(a => a.UserId == agentId);
            if (!agentExists)
                throw new AgentNotFoundException("Agent does not exist");

            var feedbacks = context.Feedback
                .AsNoTracking()
                .Where(r => r.Visit.AgentId == agentId)
                .Select(r =>  new FeedbackListDTO
                {
                    PropertyId = r.Visit.PropertyId,
                    PropertyTitle = r.Visit.Property.Title,
                    FeedbackAuthorId = r.Visit.ClientId,
                    FeedbackAuthorFirstName = r.Visit.Client.FirstName,
                    FeedbackAuthorLastName = r.Visit.Client.LastName,
                    Rating = r.Rate,
                    Message = r.Message,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return feedbacks;
        }

        private void AwardPointsToAgent(int agentId, int rating)
        {
            int points = 0;

            switch (rating)
            {
                case 5:
                    points = 15;
                    break;
                case 4:
                    points = 10;
                    break;
                default:
                    points = 0;
                    break;
            }

            if (points > 0)
                agentService.AddPoints(agentId, points);
        }
    }
}
