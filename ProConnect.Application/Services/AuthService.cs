using FluentValidation;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Core.ValueObjects;
using BCryptNet = BCrypt.Net.BCrypt;

namespace ProConnect.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginUserDto> _loginValidator;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IValidator<RegisterUserDto> registerValidator,
            IValidator<LoginUserDto> loginValidator)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto)
        {
            // Validar datos de entrada
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
            }

            try
            {
                // Crear nueva entidad User
                var user = new User
                {
                    Email = registerDto.Email.ToLowerInvariant(),
                    PasswordHash = BCryptNet.HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName.Trim(),
                    LastName = registerDto.LastName.Trim(),
                    PhoneNumber = registerDto.PhoneNumber?.Trim(),
                    UserType = registerDto.UserType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Validar dominio
                if (!user.IsValidForRegistration())
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "Los datos del usuario no son v�lidos." }
                    };
                }

                // Guardar en base de datos
                var userId = await _userRepository.CreateAsync(user);
                user.Id = userId;

                // Generar token JWT
                var token = await _jwtTokenService.GenerateTokenAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserType = user.UserType,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };
            }
            catch
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Error interno del servidor al registrar usuario." }
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto)
        {
            // Validar datos de entrada
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
            }

            try
            {
                // Buscar usuario por email
                var user = await _userRepository.GetByEmailAsync(loginDto.Email.ToLowerInvariant());
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "Credenciales inv�lidas." }
                    };
                }

                // Verificar contrase�a
                if (!BCryptNet.Verify(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "Credenciales inv�lidas." }
                    };
                }

                // Verificar que el usuario est� activo
                if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "La cuenta est� desactivada." }
                    };
                }

                // Actualizar �ltimo login
                user.UpdateLastLogin();
                await _userRepository.UpdateAsync(user);

                // Generar token JWT
                var token = await _jwtTokenService.GenerateTokenAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserType = user.UserType,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };
            }
            catch
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Error interno del servidor al iniciar sesin." }
                };
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken)
        {
            var result = await _jwtTokenService.RefreshTokenAsync(token, refreshToken);
            return new AuthResponseDto
            {
                Success = result.IsSuccess,
                Token = result.Token,
                UserId = result.UserId,
                Email = result.Email,
                UserType = result.UserType,
                ExpiresAt = result.ExpiresAt,
                Errors = result.Errors
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await _jwtTokenService.IsTokenValidAsync(token);
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            // Implementar lgica de logout si es necesaria (invalidar tokens, etc.)
            return await Task.FromResult(true);
        }
    }
}
