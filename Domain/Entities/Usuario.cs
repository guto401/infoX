namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public Usuario(string nome, string passwordHash)
        {
            nome = Nome;
            passwordHash = PasswordHash;
        }
    }
}
