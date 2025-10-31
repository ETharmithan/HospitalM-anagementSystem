using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    public class AppUserController : BaseApiController
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
