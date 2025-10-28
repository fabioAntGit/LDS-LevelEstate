#nullable disable
namespace API.ChatVisitasFeedback.Visitas.DTOs
{
    public class VisitListDTO
    {
        public int VisitId { get; set; }

        public int ClientId { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }

        public int AgentId { get; set; }
        public string AgentFirstName { get; set; }
        public string AgentLastName { get; set; }

        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }

        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
