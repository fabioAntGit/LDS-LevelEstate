using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Visitas.Exceptions;
using API.ChatVisitasFeedback.Visitas.Services;
using API.Data;
using API.Imoveis.Exceptions;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.ChatVisitasFeedback.Visitas
{
    public class VisitServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly VisitService visitService;
        private readonly int clientId;
        private readonly int agentId;
        private readonly int propertyId;

        public VisitServiceTest ()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                    .UseInMemoryDatabase($"VisitServiceDB_{Guid.NewGuid()}").Options
            );

            visitService = new VisitService(context);

            clientId = CreateClient().UserId;
            agentId = CreateAgent().UserId;
            propertyId = CreateProperty(agentId).PropertyId;
        }

        [Fact]
        public void Request_IncorrectPropertyId_ThrowsPropertyNotFoundException()
        {
            int incorrectPropertyId = 2;
            DateTime scheduledDate = DateTime.UtcNow.AddDays(2);

            Assert.Throws<PropertyNotFoundException>(() =>
                visitService.Request(clientId, incorrectPropertyId, scheduledDate)
            );
        }

        [Fact]
        public void Request_AgentCreatesOwnPropertyVisit_ThrowsAgentCannotCreateOwnPropertyVisitException()
        {
            DateTime scheduledDate = DateTime.UtcNow.AddDays(2);

            Assert.Throws<AgentCannotCreateOwnPropertyVisitException>(() =>
                visitService.Request(agentId, propertyId, scheduledDate)
            );
        }

        [Fact]
        public void Request_HasActiveVisit_ThrowsVisitAlreadyExistsException()
        {
            DateTime scheduledDate = DateTime.UtcNow.AddDays(2);

            var visit = visitService.Request(clientId, propertyId, scheduledDate);
            visitService.Accept(visit.VisitId, agentId);

            Assert.Throws<VisitAlreadyExistsException>(() =>
                visitService.Request(clientId, propertyId, scheduledDate)
            );
        }

        [Fact]
        public void Request_VisitScheduledInLessThanOneDay_ThrowsVisitDateTooSoonException()
        {
            DateTime scheduledDate = DateTime.UtcNow;

            Assert.Throws<VisitDateTooSoonException>(() =>
                visitService.Request(clientId, propertyId, scheduledDate)
            );
        }

        [Fact]
        public void Request_ValidVisit_ReturnsNewVisit()
        {
            DateTime scheduledDate = DateTime.UtcNow.AddDays(2);
            var newVisit = visitService.Request(clientId, propertyId, scheduledDate);

            Assert.NotNull(newVisit);
            Assert.Equal(clientId, newVisit.ClientId);
            Assert.Equal(agentId, newVisit.AgentId);
            Assert.Equal(propertyId, newVisit.PropertyId);
            Assert.Equal(scheduledDate, newVisit.Date);
            Assert.Equal(VisitStatus.WaitingApproval, newVisit.Status);
        }

        [Fact]
        public void Accept_InvalidVisitId_ThrowsVisitDoesntExistException()
        {
            var visitId = 1;

            Assert.Throws<VisitDoesntExistException>(() =>
                visitService.Accept(visitId, agentId)
            );
        }

        [Fact]
        public void Accept_WrongAgent_ThrowsVisitForbiddenActionException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;

            Assert.Throws<VisitForbiddenActionException>(() =>
                visitService.Accept(visitId, clientId)
            );
        }

        [Fact]
        public void Accept_VisitNotWaitingApproval_ThrowsVisitIsNotWaitingApprovalException()
        {

            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var acceptedVisit = visitService.Accept(visitId, agentId);

            Assert.Throws<VisitIsNotWaitingApprovalException>(() =>
                visitService.Accept(visitId, agentId)
            );
        }

        [Fact]
        public void Accept_ValidRequest_ReturnsAcceptedVisit()
        {

            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var acceptedVisit = visitService.Accept(visitId, agentId);

            Assert.NotNull(acceptedVisit);
            Assert.Equal(visitId, acceptedVisit.VisitId);
            Assert.Equal(VisitStatus.Accepted, acceptedVisit.Status);
        }

        [Fact]
        public void Cancel_InvalidVisitId_ThrowsVisitDoesntExistException()
        {
            var visitId = 1;

            Assert.Throws<VisitDoesntExistException>(() =>
                visitService.Cancel(visitId, agentId)
            );
        }

        [Fact]
        public void Cancel_UserNotInVisit_ThrowsVisitForbiddenActionException()
        {

            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var fakeUser = 999;
            Assert.Throws<VisitForbiddenActionException>(() =>
                visitService.Cancel(visitId, fakeUser)
            );
        }

        [Fact]
        public void Cancel_VisitWithInvalidStatus_ThrowsVisitIsNotAcceptedOrWaitingApprovalException()
        {

            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            context.Visits.Find(visitId).Status = VisitStatus.Confirmed;
            context.SaveChanges();

            Assert.Throws<VisitIsNotAcceptedOrWaitingApprovalException>(() =>
                visitService.Cancel(visitId, clientId)
            );
        }

        [Fact]
        public void Cancel_VisitAlreadyOccurred_ThrowsVisitCanceledAfterLimitException()
        {

            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            context.Visits.Find(visitId).Date = DateTime.UtcNow.AddDays(-4);
            context.SaveChanges();

            Assert.Throws<VisitCanceledAfterLimitException>(() =>
                visitService.Cancel(visitId, clientId)
            );
        }

        [Fact]
        public void Cancel_ValidRequest_ReturnsCanceledVisit()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var canceledVisit = visitService.Cancel(visitId, agentId);

            Assert.NotNull(canceledVisit);
            Assert.Equal(visitId, canceledVisit.VisitId);
            Assert.Equal(VisitStatus.Canceled, canceledVisit.Status);
        }

        [Fact]
        public void Reject_InvalidVisitId_ThrowsVisitDoesntExistException()
        {
            var visitId = 1;

            Assert.Throws<VisitDoesntExistException>(() =>
                visitService.Reject(visitId, agentId)
            );
        }

        [Fact]
        public void Reject_WrongAgent_ThrowsVisitForbiddenActionException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;

            Assert.Throws<VisitForbiddenActionException>(() =>
                visitService.Reject(visitId, clientId)
            );
        }

        [Fact]
        public void Reject_VisitNotWaitingApproval_ThrowsVisitIsNotWaitingApprovalException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var acceptedVisit = visitService.Reject(visitId, agentId);

            Assert.Throws<VisitIsNotWaitingApprovalException>(() =>
                visitService.Reject(visitId, agentId)
            );
        }

        [Fact]
        public void Reject_ValidRequest_ReturnsRejectedVisit()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var rejectedVisit = visitService.Reject(visitId, agentId);

            Assert.NotNull(rejectedVisit);
            Assert.Equal(visitId, rejectedVisit.VisitId);
            Assert.Equal(VisitStatus.Rejected, rejectedVisit.Status);
        }

        [Fact]
        public void Confirm_InvalidVisitId_ThrowsVisitDoesntExistException()
        {
            var visitId = 1;

            Assert.Throws<VisitDoesntExistException>(() =>
                visitService.Confirm(visitId, agentId)
            );
        }

        [Fact]
        public void Confirm_WrongAgent_ThrowsVisitForbiddenActionException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;

            Assert.Throws<VisitForbiddenActionException>(() =>
                visitService.Confirm(visitId, clientId)
            );
        }

        [Fact]
        public void Confirm_VisitNotAccepted_ThrowsVisitIsNotAcceptedException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;

            Assert.Throws<VisitIsNotAcceptedException>(() =>
                visitService.Confirm(visitId, agentId)
            );
        }

        [Fact]
        public void Confirm_VisitBeforeDate_ThrowsVisitConfirmBeforeOccurredException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            context.Visits.Find(visitId).Status = VisitStatus.Accepted;
            context.SaveChanges();

            Assert.Throws<VisitConfirmBeforeOccurredException>(() =>
                visitService.Confirm(visitId, agentId)
            );
        }

        [Fact]
        public void Confirm_ValidRequest_ReturnsConfirmedVisit()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            context.Visits.Find(visitId).Date = DateTime.UtcNow.AddDays(-3);
            context.Visits.Find(visitId).Status = VisitStatus.Accepted;
            context.SaveChanges();
            var ConfirmedVisit = visitService.Confirm(visitId, agentId);

            Assert.NotNull(ConfirmedVisit);
            Assert.Equal(visitId, ConfirmedVisit.VisitId);
            Assert.Equal(VisitStatus.Confirmed, ConfirmedVisit.Status);
        }

        [Fact]
        public void List_ClientVisits_ReturnsAllVisitsOfClient()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var propertyId2 = CreateProperty(agentId).PropertyId;
            var visitId2 = CreateVisit(clientId, agentId, propertyId2).VisitId;

            var propertyId3 = CreateProperty(agentId).PropertyId;
            var visitId3 = CreateVisit(clientId, agentId, propertyId3).VisitId;

            var visits = visitService.List(clientId);

            Assert.NotEmpty(visits);
            Assert.Equal(3, visits.Count());
            Assert.All(visits, v => Assert.True(v.ClientId == clientId && v.AgentId == agentId));
        }

        [Fact]
        public void Get_NonExistingChatId_ThrowsChatNotFoundException()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;

            var chatId = 1;

            Assert.Throws<ChatNotFoundException>(() =>
                visitService.Get(chatId)
            );
        }

        [Fact]
        public void Get_VisitDoesNotExist_ThrowsVisitDoesntExistException()
        {
            var chatId = CreateChat().ChatId;

            Assert.Throws<VisitDoesntExistException>(() =>
                visitService.Get(chatId)
            );
        }

        [Fact]
        public void Get_VisitOfChat_ReturnsLatestVisit()
        {
            var visitId = CreateVisit(clientId, agentId, propertyId).VisitId;
            var visitId2 = CreateVisit(clientId, agentId, propertyId).VisitId;

            var chatId = CreateChat().ChatId;

            var visit = visitService.Get(chatId);

            Assert.NotNull(visit);
            Assert.Equal(visitId2, visit.VisitId);
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
                FirstName = "João",
                LastName = "Silva",
                Phone = "+351912345678",
                Email = "joao.silva@imobiliaria.pt",
                Password = "Password123!",
                AMICode = "AMI12345",
                ReferralCode = "REF12345",
                MonthlyPoints = 0,
                AnnualPoints = 0,
                AllMoneyEarned = 0,
                MoneyEarnedLastMonth = 0,
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.Verified,
                Role = Role.Agent
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
                Email = "joao.silva.client@exemplo.pt",
                Password = "Password123!",
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.Verified,
                Role = Role.Client,
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

        private API.Models.Chat CreateChat()
        {
            var chat = new API.Models.Chat
            {
                ClientId = clientId,
                AgentId = agentId,
                PropertyId = propertyId,
                CreatedAt = DateTime.UtcNow
            };

            context.Chats.Add(chat);
            context.SaveChanges();

            return chat;
        }
    }
}
