namespace UnitOfWork.WebAPI.Models
{
    public class SignUpModel
    {
        public string? Id { get; set; }
        public string Username { get; set; }
        public int Category { get; set; }
        public string? Password { get; set; }
        public string? IPAddress { get; set; }
        public bool? IsDCP { get; set; }
        public string? Formation { get; set; }
        public int? PayScale { get; set; }
    }
}
