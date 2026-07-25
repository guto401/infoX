namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public Usuario() { }

        public Usuario(string nome, string passwordHash)
        {
            Nome = nome;         
            PasswordHash = passwordHash;
        }
    }
}
