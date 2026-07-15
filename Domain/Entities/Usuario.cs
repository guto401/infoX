namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public int Nome { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
    }
}
