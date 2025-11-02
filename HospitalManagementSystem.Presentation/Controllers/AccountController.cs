using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    public class AccountController : BaseApiController
    {
        [HttpGet]
        public ActionResult<string> Login()   // 
        {
            return "Login Successful";
        }
    }
}
