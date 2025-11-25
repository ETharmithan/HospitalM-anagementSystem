using HospitalManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.IServices
{
    public interface ITokenService
    {
        string CreateToken(User Patient);
    }
}
