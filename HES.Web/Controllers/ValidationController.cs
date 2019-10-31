using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidationController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public ValidationController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> VerifyEmail([Bind(Prefix = "EmployeeWizard.Employee.Email")]string email)
        {
            var employee = await _employeeService.Query().FirstOrDefaultAsync(e => e.Email == email);

            if (employee != null)
            {
                return new JsonResult($"Email {email} already in use.");
            }

            return new JsonResult(true);
        }
    }
}