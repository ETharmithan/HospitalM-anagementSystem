using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.IServices;
using HospitalManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Infrastructure.Extensions
{
    public static class AppUserExtensions
    {
        public static User_Dto ToDto(this User user, ITokenService tokenService)
        {
            return new User_Dto
            {
                Id = user.UserId.ToString(),
                Email = user.Email,
                DisplayName = user.Username,
                ImageUrl = user.ImageUrl,
                Token = tokenService.CreateToken(user),
                Role = user.Role
            };
        }


    }
}
