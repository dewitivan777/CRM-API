using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthService.Models;

namespace AuthService.Controllers
{ /// <summary>
  /// Provides enpoint to add and manage user identities
  /// </summary>
    [ApiController]
    [Route("service/account")]
    public class AccountController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IRepository<PasswordResetToken> _repository;
        private readonly IRepository<ApiToken> _tokenRepository;

        /// <summary>
        /// Initialize a new instance of <see cref="AccountController"/>
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="repository"></param>
        /// <param name="tokenRepository"></param>
        public AccountController(
            IUserManager userManager,
            IRepository<PasswordResetToken> repository,
            IRepository<ApiToken> tokenRepository)
        {
            _userManager = userManager;
            _repository = repository;
            _tokenRepository = tokenRepository;
        }

        /// <summary>
        /// register new account
        /// </summary>
        /// <param name="accountRegisterModel"></param>
        /// <returns></returns>
        /// POST service/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]AccountRegisterModel accountRegisterModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrEmpty(accountRegisterModel.Username))
                accountRegisterModel.Username = accountRegisterModel.Email;

            var user = new IdentityUser(accountRegisterModel.Username, accountRegisterModel.Email);

            if (accountRegisterModel.Claims != null)
            {
                foreach (string claim in accountRegisterModel.Claims)
                {
                    user.AddClaim(new UserClaim(JwtClaimTypes.Role, claim));
                }
            }

            var result = await _userManager.CreateAsync(user, accountRegisterModel.Password);

            if (result == null) return StatusCode(500, "Internal Server Error");

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(accountRegisterModel.Cell))
                {
                    await _userManager.SetPhoneNumberAsync(user, accountRegisterModel.Cell);
                }

                return Ok(new
                {
                    user.Id,
                    user.CreatedOn
                });
            }

            AddErrorsToModelState(result);

            if (ModelState.IsValid) return BadRequest();

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordDto entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(entity.Username);

            if (user == null)
            {
                return NotFound("User not found");
            }


            if (User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value?.ToLower() == AuthDefaults.ModeratorRoleName || c.Value?.ToLower() == AuthDefaults.RootRoleName)))
            {
                var entityResult = await _userManager.ResetPasswordAsync(user, entity.NewPassword);

                return Ok(entityResult);
            }
            else
            {
                if (string.IsNullOrEmpty(entity.CurrentPassword))
                {
                    return Ok(new IdentityResult
                    {
                        Succeeded = false,
                        Errors = new List<string>
                        {
                            "Current Password is required"
                        }
                    });
                }

                var result = await _userManager.ChangePasswordAsync(user, entity.CurrentPassword, entity.NewPassword);

                return Ok(result);
            }
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("resetpasswordwithtoken")]
        public async Task<IActionResult> ResetPasswordWithToken([FromBody]PasswordResetTokenDto entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repository.GetByIdAsync(entity.Token);

            if (result == null || result.RequestedOn < DateTime.Now.AddDays(-1) || string.Compare(entity.Email, result.Email, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return BadRequest("Token does not exist or has expired");
            }

            var user = await _userManager.FindByNameAsync(entity.Email);

            if (user == null)
            {
                return Ok(entity);
            }

            var entityResult = await _userManager.ResetPasswordAsync(user, entity.NewPassword);

            _repository.Delete(result.Id);

            return Ok(entity);
        }

        /// <summary>
        /// Update user claims
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("claims")]
        public async Task<IActionResult> UpdateClaims([FromBody]ClaimsDto entity)
        {
            var user = await _userManager.FindByIdAsync(entity.UserId);

            if (user == null)
                return NotFound();

            var removeClaims = new List<UserClaim>();

            foreach (var claim in user.Claims ?? new List<UserClaim>())
            {
                if (claim.Type != JwtClaimTypes.Subject || claim.Type != JwtClaimTypes.Role)
                    continue;

                if (!entity.Claims.Contains(claim.Value, StringComparer.OrdinalIgnoreCase))
                {
                    removeClaims.Add(claim);
                }
            }

            foreach (var claim in removeClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            foreach (string newClaim in entity.Claims)
            {
                if (!user.Claims.Any(tbl => (tbl.Type == JwtClaimTypes.Role || tbl.Type == JwtClaimTypes.Subject) && tbl.Value.ToLower() == newClaim.ToLower()))
                {
                    await _userManager.AddClaimAsync(user, new UserClaim(JwtClaimTypes.Role, newClaim));
                }
            }

            // clean after
            // converting subs to roles, and reserving sub for user id
            var swapClaims = new List<UserClaim>();
            var dupeClaims = new List<UserClaim>();

            foreach (var c in user.Claims ?? new List<UserClaim>())
            {
                if (c.Type == JwtClaimTypes.Subject)
                {
                    if (user.Claims.Any(claim => (claim.Type == JwtClaimTypes.Role && claim.Value == c.Value)))
                    {
                        dupeClaims.Add(c);
                    }
                    else
                    {
                        swapClaims.Add(c);
                    }
                }
            }

            // Remove dupes
            foreach (var claim in dupeClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            // Add swap claims
            foreach (var claim in swapClaims)
            {
                await _userManager.AddClaimAsync(user, new UserClaim(JwtClaimTypes.Role, claim.Value));
            }

            return Ok(entity);
        }

        /// <summary>
        /// Get user claims
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("claims")]
        public async Task<IActionResult> GetClaims(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(new ClaimsDto()
            {
                UserId = id,
                Claims = user.Claims.Where(tbl => (tbl.Type == JwtClaimTypes.Role || tbl.Type == JwtClaimTypes.Subject)).Select(tbl => tbl.Value).ToList()
            });
        }

        /// <summary>
        /// Create user token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("CreateToken")]
        public IActionResult CreateToken(string userId)
        {
            var entity = new ApiToken()
            {
                UserId = userId,
                Id = Guid.NewGuid().ToString().Replace("-", "")
            };

            _tokenRepository.Add(entity);

            return Ok(entity);
        }

        /// <summary>
        /// Get or create user token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("GetOrCreateToken")]
        [Authorize(Policy.EntityOwnerOrModerator)]
        public async Task<IActionResult> GetOrCreateToken(string userId)
        {
            var token = await _tokenRepository.ListAsync(tbl => tbl.UserId == userId);

            if (token != null && token.Count > 0)
            {
                return Ok(token.FirstOrDefault());
            }

            var entity = new ApiToken()
            {
                UserId = userId,
                Id = Guid.NewGuid().ToString().Replace("-", "")
            };

            _tokenRepository.Add(entity);

            return Ok(entity);
        }

        /// <summary>
        /// Get user by: username, userId or token
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(string username, string userId, string token)
        {
            IdentityUser result = null;

            if (!string.IsNullOrEmpty(username))
            {
                username = username.ToLower();
                result = await _userManager.FindByNameAsync(username);
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                result = await _userManager.FindByIdAsync(userId);
            }
            else if (!string.IsNullOrEmpty(token))
            {
                var tokenUser = await _tokenRepository.GetByIdAsync(token);

                if (tokenUser != null)
                {
                    result = await _userManager.FindByIdAsync(tokenUser.UserId);
                }
            }

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("changeemail")]
        public async Task<IActionResult> ChangeEmail([FromBody]ChangeEmailDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound("user not found");
            }

            var result = await _userManager.ChangeEmailAsync(user, model.Email);

            if (result.Succeeded)
            {
                return Ok(model);
            }

            AddErrorsToModelState(result);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return StatusCode(500);
        }

        private void AddErrorsToModelState(IdentityResult result)
        {
            foreach (var error in result.Errors ?? new List<string>())
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
    }
}