using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Visitas.DTOs;
using API.ChatVisitasFeedback.Visitas.Exceptions;
using API.ChatVisitasFeedback.Visitas.Interfaces;
using API.Data;
using API.Imoveis.Exceptions;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.ChatVisitasFeedback.Visitas.Services
{
    public class VisitService : IVisitService
    {

        private readonly LevelEstateContext context;

        public VisitService(LevelEstateContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="VisitDoesntExistException"></exception>
        /// <exception cref="UserIsNotTheAgentOfThisVisitException"></exception>
        /// <exception cref="VisitIsNotWaitingApprovalException"></exception>
        public Visit Accept(int visitId, int userId)
        {
            var visit = GetVisitOrThrow(visitId);

            if (visit.AgentId != userId)
                throw new VisitForbiddenActionException("User is not the agent of this visit");

            if (visit.Status != VisitStatus.WaitingApproval)
                throw new VisitIsNotWaitingApprovalException("The visit is not waiting approval");

            visit.Status = VisitStatus.Accepted;
            context.SaveChanges();

            return visit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="VisitDoesntExistException"></exception>
        /// <exception cref="VisitForbiddenActionException"></exception>
        /// <exception cref="VisitIsNotAcceptedOrWaitingApprovalException"></exception>
        /// <exception cref="VisitCanceledAfterLimitException"></exception>
        public Visit Cancel(int visitId, int userId)
        {
            var visit = GetVisitOrThrow(visitId);

            if (visit.ClientId != userId && visit.AgentId != userId)
                throw new VisitForbiddenActionException("User is not authorized to cancel this visit");

            if (visit.Status != VisitStatus.Accepted && visit.Status != VisitStatus.WaitingApproval)
                throw new VisitIsNotAcceptedOrWaitingApprovalException("Only accepted or waiting approval visits can be canceled");

            if (visit.Date <= DateTime.UtcNow)
                throw new VisitCanceledAfterLimitException("Cannot cancel a visit that has already occurred");

            visit.Status = VisitStatus.Canceled;
            context.SaveChanges();

            return visit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="VisitDoesntExistException"></exception>
        /// <exception cref="VisitForbiddenActionException"></exception>
        /// <exception cref="VisitIsNotAcceptedException"></exception>
        /// <exception cref="VisitConfirmBeforeOccurredException"></exception>
        public Visit Confirm(int visitId, int userId)
        {
            var visit = GetVisitOrThrow(visitId);

            if (visit.AgentId != userId)
                throw new VisitForbiddenActionException("User is not authorized to confirm this visit");

            if (visit.Status != VisitStatus.Accepted)
                throw new VisitIsNotAcceptedException("Only accepted visits can be confirmed");

            if (visit.Date > DateTime.UtcNow)
                throw new VisitConfirmBeforeOccurredException("Cannot confirm a visit that hasn't occurred yet");

            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();
            return visit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        /// <exception cref="ChatNotFoundException"></exception>
        /// <exception cref="VisitDoesntExistException"></exception>
        public Visit Get(int chatId)
        {
            var chat = context.Chats.Find(chatId);

            if (chat == null)
                throw new ChatNotFoundException($"Chat with id {chatId} Not Found");

            var visit = context.Visits
                .Include(v => v.Feedback)
                .Where(v => v.AgentId == chat.AgentId && v.ClientId == chat.ClientId && v.PropertyId == chat.PropertyId)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();

            if (visit == null)
                throw new VisitDoesntExistException("Visit not found");

            return visit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<VisitListDTO> List(int userId)
        {

            var visits = context.Visits
                .Include(v => v.Property)
                .Include(v => v.Client)
                .Include(v => v.Agent)
                .Where(v => v.ClientId == userId || v.AgentId == userId)
                .Select(v => new VisitListDTO
                {
                    VisitId = v.VisitId,
                    PropertyId = v.PropertyId,
                    PropertyTitle = v.Property.Title,
                    ClientId = v.ClientId,
                    ClientFirstName = v.Client.FirstName,
                    ClientLastName = v.Client.LastName,
                    AgentId = v.AgentId,
                    AgentFirstName = v.Agent.FirstName,
                    AgentLastName = v.Agent.LastName,
                    Date = v.Date,
                    Status = v.Status.ToString()
                })
                .ToList();

            return visits;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="VisitDoesntExistException"></exception>
        /// <exception cref="UserIsNotTheAgentOfThisVisitException"></exception>
        /// <exception cref="VisitIsNotWaitingApprovalException"></exception>
        public Visit Reject(int visitId, int userId)
        {
            var visit = GetVisitOrThrow(visitId);

            if (visit.AgentId != userId)
                throw new VisitForbiddenActionException("User is not the agent of this visit");

            if (visit.Status != VisitStatus.WaitingApproval)
                throw new VisitIsNotWaitingApprovalException("The visit is not waiting approval");

            visit.Status = VisitStatus.Rejected;
            context.SaveChanges();
            return visit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        /// <exception cref="PropertyNotFoundException"></exception>
        /// <exception cref="AgentCannotCreateOwnPropertyVisitException"></exception>
        /// <exception cref="VisitAlreadyExistsException"></exception>
        public Visit Request(int userId, int propertyId, DateTime scheduledDate)
        {
            var property = context.Properties.Find(propertyId);

            if (property == null)
                throw new PropertyNotFoundException($"Property with id {propertyId} not found.");

            var isUserTheAgent = context.Properties.Any(p => p.PropertyId == propertyId && p.AgentId == userId);

            if (isUserTheAgent)
                throw new AgentCannotCreateOwnPropertyVisitException("Agent cannot request a visit to their own property");

            var visitExist = context.Visits.Any(v =>
                v.PropertyId == propertyId &&
                v.ClientId == userId &&
                (
                    v.Status == VisitStatus.WaitingApproval ||
                    v.Status == VisitStatus.Accepted
                ));

            if (visitExist)
                throw new VisitAlreadyExistsException("A visit for this property by the same user already exists and is either Waiting for Approval or Accepted");

            if (scheduledDate < DateTime.UtcNow.AddDays(1))
                throw new VisitDateTooSoonException("Visits must be scheduled at least 1 day in advance");

            var newVisit = new Visit
            {
                ClientId = userId,
                AgentId = property.AgentId,
                PropertyId = propertyId,
                Date = scheduledDate,
                Status = VisitStatus.WaitingApproval,
                CreatedAt = DateTime.UtcNow
            };

            context.Visits.Add(newVisit);
            context.SaveChanges();

            return newVisit;
        }

        private Visit GetVisitOrThrow(int visitId)
        {
            var visit = context.Visits.Find(visitId);

            if (visit == null)
                throw new VisitDoesntExistException("This visit doesnt exist");

            return visit;
        }
    }
}