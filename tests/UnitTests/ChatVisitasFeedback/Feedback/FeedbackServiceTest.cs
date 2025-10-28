using API.ChatVisitasFeedback.Feedbacks.Exceptions;
using API.ChatVisitasFeedback.Feedbacks.Services;
using API.ChatVisitasFeedback.Visitas.Exceptions;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Services;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.ChatVisitasFeedback.Feedbacks
{
    public class FeedbackServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly FeedbackService feedbackService;
        private readonly AgentService agenteService;
        private readonly int propertyId;
        private readonly int clientId;
        private readonly int agentId;
        private readonly Visit visit;

        public FeedbackServiceTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                   .UseInMemoryDatabase($"FeedbackServiceDB_{Guid.NewGuid()}").Options
           );
            agenteService = new AgentService(context);
            feedbackService = new FeedbackService(context, agenteService);

            agentId = CreateAgent().UserId;
            clientId = CreateClient().UserId;
            propertyId = CreateProperty(agentId).PropertyId;
            visit = CreateVisit(clientId, agentId, propertyId);
        }

        [Fact]
        public void Submit_InvalidVisitId_ThrowsVisitDoesntExistException()
        {
            var visitId = 9999;

            Assert.Throws<VisitDoesntExistException>(() =>
            {
                feedbackService.Submit(clientId, visitId, 5, "Great visit!");
            });
        }

        [Fact]
        public void Submit_VisitNotConfirmed_ThrowsVisitNotConfirmedException()
        {
            Assert.Throws<VisitNotConfirmedException>(() =>
            {
                feedbackService.Submit(clientId, visit.VisitId, 5, "Great visit!");
            });
        }

        [Fact]
        public void Submit_UnauthorizedUser_ThrowsFeedbackForbiddenActionException()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            Assert.Throws<FeedbackForbiddenActionException>(() =>
            {
                feedbackService.Submit(agentId, visit.VisitId, 5, "Great visit!");
            });
        }

        [Fact]
        public void Submit_ResendFeedback_ThrowsFeedbackAlreadyExistsException()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            CreateFeedback(visit.VisitId, 4, "Good visit");

            Assert.Throws<FeedbackAlreadyExistsException>(() =>
            {
                feedbackService.Submit(clientId, visit.VisitId, 5, "Great visit!");
            });
        }

        [Fact]
        public void Submit_SendIncorrectRating_ThrowsFeedbackInvalidRatingException()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            Assert.Throws<FeedbackInvalidRatingException>(() =>
            {
                feedbackService.Submit(clientId, visit.VisitId, 6, "Great visit!");
            });
        }

        [Fact]
        public void Submit_SendMessageWithMoreThan150Characters_ThrowsFeedbackMessageTooLongException()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            string message = new string('a', 151);

            Assert.Throws<FeedbackMessageTooLongException>(() =>
            {
                feedbackService.Submit(clientId, visit.VisitId, 5, message);
            });
        }

        [Fact]
        public void Submit_ValidFeedback_ReturnsNewFeedback()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            Feedback newFeedback = feedbackService.Submit(clientId, visit.VisitId, 5, "Great visit!");

            Assert.NotNull(newFeedback);
            Assert.Equal(visit.VisitId, newFeedback.VisitId);
            Assert.Equal(5, newFeedback.Rate);
            Assert.Equal("Great visit!", newFeedback.Message);
            Assert.Equal(clientId, newFeedback.Visit.ClientId);
        }

        [Fact]
        public void Submit_NullOrEmptyMessage_StoresAsNull()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            var feedback = feedbackService.Submit(clientId, visit.VisitId, 5, "   ");

            Assert.Null(feedback.Message);
        }

        [Fact]
        public void Submit_CreateFeedback_ReturnsFeedbackLinkedToVisit()
        {
            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            var feedback = feedbackService.Submit(clientId, visit.VisitId, 5, "Good visit");

            var visits = context.Visits
                .Include(v => v.Feedback)
                .First(v => v.VisitId == visit.VisitId);

            Assert.NotNull(visits.Feedback);
            Assert.Equal(feedback.FeedbackId, visits.Feedback.FeedbackId);
        }

        [Fact]
        public void Submit_PositiveFeedback_AddsPointsToAgent()
        {
            var agent = context.Agents.First();
            var initialPoints = agent.MonthlyPoints;

            visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            feedbackService.Submit(clientId, visit.VisitId, 5, "Excelente atendimento!");

            var updatedAgent = context.Agents.First(a => a.UserId == agent.UserId);
            Assert.Equal(initialPoints + 15, updatedAgent.MonthlyPoints);
        }


        [Fact]
        public void List_AgentInvalidId_ThrowsAgentNotFoundException()
        {
            var invalidAgentId = 9999;

            Assert.Throws<AgentNotFoundException>(() =>
            {
                feedbackService.List(invalidAgentId);
            });
        }

        [Fact]
        public void List_WithMultipleFeedbacks_ReturnsAllAgentFeedbacks()
        {
			//first feedback
			visit.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            var feedback = CreateFeedback(visit.VisitId, 5, "Excellent visit");

			//second feedback
			var visit2 = CreateVisit(clientId, agentId, propertyId);

            visit2.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            var feedback2 = CreateFeedback(visit2.VisitId, 5, "Excellent visit");

			//third feedback
			var visit3 = CreateVisit(clientId, agentId, propertyId);

            visit3.Status = VisitStatus.Confirmed;
            context.SaveChanges();

            var feedback3 = CreateFeedback(visit3.VisitId, 5, "Excellent visit");

            var feedbacks = feedbackService.List(agentId);
            Assert.NotEmpty(feedbacks);
            Assert.Equal(3, feedbacks.Count());
            Assert.All(feedbacks, f =>
            {
                Assert.NotNull(f.PropertyTitle);
                Assert.NotNull(f.FeedbackAuthorFirstName);
                Assert.NotNull(f.FeedbackAuthorLastName);
                Assert.InRange(f.Rating, 1, 5);
            });
        }

        private Property CreateProperty(int agentId)
        {
            var property = new Property
            {
                AgentId = agentId,
                Street = "Rua dos Testes 123",
                PostalCode = "1000-001",
                Latitude = 38.7169m,
                Longitude = -9.1399m,
                Title = "Apartamento T2 de Teste",
                Price = 1000000,
                Status = Status.Available,
                Topology = Topologies.Apartment,
                TypeOperation = TypeOperations.Sale,
                Area = 85.5m,
                Floor = 3,
                NumberOfFloors = 5,
                Bedrooms = 2,
                Bathrooms = 1,
                BuiltAt = 2015,
                CreatedAt = DateTime.UtcNow
            };

            context.Properties.Add(property);
            context.SaveChanges();

            return property;
        }

        private Agent CreateAgent()
        {
            var agent = new Agent
            {
                FirstName = "Fabio",
                LastName = "Antunes",
                Phone = "+351912345678",
                Email = "fabio.antunes@test.pt",
                Password = "Password123!",
                AMICode = "AMI12345",
                ReferralCode = "REF12345",
                MonthlyPoints = 0,
                AnnualPoints = 0,
                AllMoneyEarned = 0,
                MoneyEarnedLastMonth = 0,
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.Verified
            };

            context.Users.Add(agent);
            context.SaveChanges();

            return agent;
        }

        private Client CreateClient()
        {
            var client = new Client
            {
                FirstName = "Cristiano",
                LastName = "Ronaldo",
                Phone = "+351912345678",
                Email = "cristiano.ronaldo@test.pt",
                Password = "Password123!",
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.Verified
            };

            context.Users.Add(client);
            context.SaveChanges();

            return client;
        }

        private Visit CreateVisit(int clientId, int agentId, int propertyId)
        {
            var visit = new Visit
            {
                ClientId = clientId,
                AgentId = agentId,
                PropertyId = propertyId,
                Date = DateTime.UtcNow.AddDays(2),
                Status = VisitStatus.WaitingApproval,
                CreatedAt = DateTime.UtcNow
            };

            context.Visits.Add(visit);
            context.SaveChanges();

            return visit;
        }

        private Feedback CreateFeedback(int visitId, byte rating, string message)
        {
            var feedback = new Feedback
            {
                VisitId = visitId,
                Rate = rating,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            context.Feedback.Add(feedback);
            context.SaveChanges();

            return feedback;
        }
    }
}
