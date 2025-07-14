using FluentValidation;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using ProConnect.Core.ValueObjects;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Security.Cryptography;
using System.Text;

namespace ProConnect.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginUserDto> _loginValidator;
        private readonly IEmailService _emailService;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IValidator<RegisterUserDto> registerValidator,
            IValidator<LoginUserDto> loginValidator,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _emailService = emailService;
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
                        Errors = new List<string> { "Los datos del usuario no son válidos." }
                    };
                }

                // Generar token de verificación de email
                user.EmailVerificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
                user.EmailVerified = false;

                // Guardar en base de datos
                var userId = await _userRepository.CreateAsync(user);
                user.Id = userId;

                // Enviar email de verificación
                await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken);

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
                        Errors = new List<string> { "Credenciales inválidas." }
                    };
                }

                // Verificar contraseña
                if (!BCryptNet.Verify(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "Credenciales inválidas." }
                    };
                }

                // Verificar que el email esté verificado
                if (!user.EmailVerified)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "Debes verificar tu correo antes de iniciar sesión. Revisa tu email o solicita un nuevo enlace de verificación." }
                    };
                }

                // Verificar que el usuario esté activo
                if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Errors = new List<string> { "La cuenta está desactivada." }
                    };
                }

                // Actualizar último login
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
                    Errors = new List<string> { "Error interno del servidor al iniciar sesión." }
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
            // Implementar lógica de logout si es necesaria (invalidar tokens, etc.)
            return await Task.FromResult(true);
        }

        public async Task<bool> SendEmailVerificationAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());
            if (user == null) return false;
            // Si ya está verificado, no reenviar
            if (user.EmailVerified) return true;
            // Si el token expiró o no existe, generar uno nuevo
            if (string.IsNullOrEmpty(user.EmailVerificationToken) || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            {
                user.EmailVerificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
                await _userRepository.UpdateAsync(user);
            }
            await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken);
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.EmailVerificationToken == token);
            if (user == null)
            {
                // Log de intento fallido
                // _logger?.LogWarning($"Intento de verificación con token inválido: {token}");
                return false;
            }
            if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            {
                // Log de intento fallido por expiración
                // _logger?.LogWarning($"Token expirado para usuario: {user.Email}");
                // Invalidar token
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiresAt = null;
                await _userRepository.UpdateAsync(user);
                return false;
            }
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;
            return new UserProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.PhoneNumber ?? string.Empty,
                Bio = user.Bio ?? string.Empty,
                Email = user.Email
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto)
        {
            // Validar datos de entrada
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) || string.IsNullOrWhiteSpace(dto.Phone))
            {
                Console.WriteLine($"[AUDITORIA] Fallo al actualizar perfil: campos obligatorios vacios para usuario {userId} ({DateTime.UtcNow})");
                return false;
            }
            if (dto.Bio != null && dto.Bio.Length > 500)
            {
                Console.WriteLine($"[AUDITORIA] Fallo al actualizar perfil: bio demasiado larga para usuario {userId} ({DateTime.UtcNow})");
                return false;
            }
            // Validación de teléfono básica
            if (dto.Phone.Length > 20)
            {
                Console.WriteLine($"[AUDITORIA] Fallo al actualizar perfil: telefono demasiado largo para usuario {userId} ({DateTime.UtcNow})");
                return false;
            }
            // Actualizar en base de datos
            var result = await _userRepository.UpdateProfileFieldsAsync(userId, dto.FirstName, dto.LastName, dto.Phone, dto.Bio ?? string.Empty);
            // Logging básico
            if (result)
                Console.WriteLine($"[AUDITORIA] Usuario {userId} actualizo su perfil correctamente ({DateTime.UtcNow})");
            else
                Console.WriteLine($"[AUDITORIA] Error al actualizar perfil en base de datos para usuario {userId} ({DateTime.UtcNow})");
            return result;
        }
    }
}
