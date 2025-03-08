using Microsoft.AspNetCore.Identity;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    //27.3 triển khai tạo user identity
    public interface IAuthenticationService
    {
        Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration);
        //27.8 triển khai Authentication
        Task<bool> ValidateUser(UserForAuthenticationDto userForAuth);

        //28.2 string -> token dto
        Task<TokenDto> CreateToken(bool populateExp);

        Task<TokenDto> RefreshToken(TokenDto tokenDto);
    }
}
