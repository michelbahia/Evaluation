namespace WebApplication1.Models
{
    public class TodoItemDTO
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public long CardNumber { get; set; }
        public int CVV { get; set; }
        public DateTime RegistrationDate { get; set; }
        public long Token { get; set; }
        public int CardId { get; set; }
    }
}
