using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    public class AccountController : BaseApiController
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
