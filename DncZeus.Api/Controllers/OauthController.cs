using DncZeus.Api.Entities;
using DncZeus.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using DncZeus.Api.Auth;
using static DncZeus.Api.Entities.Enums.CommonEnum;
using System;

namespace DncZeus.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly AppAuthenticationSettings _appSettings;
        private readonly DncZeusDbContext _dbContext;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="dbContext"></param>
        public OauthController(IOptions<AppAuthenticationSettings> appSettings, DncZeusDbContext dbContext)
        {
            _appSettings = appSettings.Value;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Auth(string username, string password)
        {
            var response = ResponseModelFactory.CreateInstance;
            DncUser user;
            using (_dbContext)
            {
                user = _dbContext.DncUser.FirstOrDefault(x => x.LoginName == username.Trim());
                if (user == null || user.IsDeleted == IsDeleted.Yes)
                {
                    response.SetFailed("用户不存在");
                    return Ok(response);
                }
                if (user.Password != U8Encrypt((password ?? "").Trim()))
                {
                    response.SetFailed("密码不正确");
                    return Ok(response);
                }
                if (user.IsLocked == IsLocked.Locked)
                {
                    response.SetFailed("账号已被锁定");
                    return Ok(response);
                }
                if (user.Status == UserStatus.Forbidden)
                {
                    response.SetFailed("账号已被禁用");
                    return Ok(response);
                }
            }
            var claimsIdentity = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("guid",user.FGuid.ToString()),
                    new Claim("avatar",""),
                    new Claim("displayName",user.DisplayName),
                    new Claim("loginName",user.LoginName),
                    new Claim("emailAddress",""),
                    new Claim("guid",user.FGuid.ToString()),
                    new Claim("userType",((int)user.UserType).ToString())
                });
            var token = JwtBearerAuthenticationExtension.GetJwtAccessToken(_appSettings, claimsIdentity);

            response.SetData(token);
            return Ok(response);
        }

        public string U8Encrypt(string input)
        {
            string rethash = "";
            try
            {
                System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
                System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                byte[] combined = encoder.GetBytes(input);
                hash.ComputeHash(combined);
                rethash = Convert.ToBase64String(hash.Hash);
            }
            catch (Exception)
            {
                return "";
            }
            //用友密码最后一位补位 
            return rethash + (char)3;
        }
    }
}