namespace MiniPay.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId {  get; set; }
        public User User { get; set; }

        public string Description { get; set; }
    }
}
