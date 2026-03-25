using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Request
{
    public class UserPwdUpdateRequest
    {
        public Guid UserId { get; set; }
        public string NewPwd { get; set; }
        public string OldPwd { get; set; }
    }
}
