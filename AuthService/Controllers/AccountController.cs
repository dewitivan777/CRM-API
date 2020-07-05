using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Repositories;
using AuthService.Services;
using IdentityMicroservice.Model;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository<User> _userRepository;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly IEncryptor _encryptor;

        public AccountController(IUserRepository<User> userRepository, IJwtBuilder jwtBuilder, IEncryptor encryptor)
        {
            _userRepository = userRepository;
            _jwtBuilder = jwtBuilder;
            _encryptor = encryptor;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] User user)
        {
            var u = await _userRepository.GetByIdAsync(user.Id);

            if (u == null)
            {
                return NotFound("User not found.");
            }

            var isValid = u.ValidatePassword(user.Password, _encryptor);

            if (!isValid)
            {
                return BadRequest("Could not authenticate user.");
            }

            var token = _jwtBuilder.GetToken(u.Id);

            return Ok(token);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] User user)
        {
            var u = await _userRepository.GetByIdAsync(user.Id);

            if (u != null)
            {
                return BadRequest("User already exists.");
            }

            user.SetPassword(user.Password, _encryptor);
            _userRepository.Add(user);

            return Ok();
        }

        [HttpGet("validate")]
        public async Task<ActionResult<string>> Validate([FromQuery(Name = "id")] string id, [FromQuery(Name = "token")] string token)
        {
            var u = await _userRepository.GetByIdAsync(id);

            if (u == null)
            {
                return NotFound("User not found.");
            }

            var userId = _jwtBuilder.ValidateToken(token);

            if (userId != u.Id)
            {
                return BadRequest("Invalid token.");
            }

            return Ok(userId);
        }
    }
}