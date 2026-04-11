namespace UnitOfWork.WebAPI.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        public string RoleName { get; set; }

        public bool Read { get; set; }

        public bool Write { get; set; }
        public bool Update { get; set; }

        public bool Delete { get; set; }
        public bool Approval { get; set; }
    }
}
