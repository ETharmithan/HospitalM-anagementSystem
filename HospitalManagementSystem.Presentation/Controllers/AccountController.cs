using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    // [Authorize]
    public class AccountController : BaseApiController
    {
        [HttpGet]
        public ActionResult<string> Login()   // 
        {
            return "Login Successful";
        }
    }
}
