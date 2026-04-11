namespace UnitOfWork.WebAPI.Models
{
    public class ChangePasswordViewModel
    {
        public string Id { get; set; }
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
