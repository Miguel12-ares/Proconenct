// Validación client-side para formularios de autenticación
class AuthValidation {
    constructor() {
        this.initializeValidation();
        this.initializeLoadingStates();
        this.initializeFocusStates();
    }

    initializeValidation() {
        // Validación de registro
        const registerForm = document.querySelector('.register-form');
        if (registerForm) {
            this.setupRegisterValidation(registerForm);
        }

        // Validación de login
        const loginForm = document.querySelector('.login-form');
        if (loginForm) {
            this.setupLoginValidation(loginForm);
        }
    }

    setupRegisterValidation(form) {
        const emailInput = form.querySelector('#Email');
        const passwordInput = form.querySelector('#Password');
        const confirmPasswordInput = form.querySelector('#ConfirmPassword');
        const firstNameInput = form.querySelector('#FirstName');
        const lastNameInput = form.querySelector('#LastName');
        const phoneInput = form.querySelector('#PhoneNumber');
        const userTypeInputs = form.querySelectorAll('input[name="UserType"]');

        // Validación de email en tiempo real
        emailInput.addEventListener('blur', () => {
            this.validateEmail(emailInput);
        });

        // Validación de contraseña en tiempo real
        passwordInput.addEventListener('input', () => {
            this.validatePassword(passwordInput);
        });

        // Validación de confirmación de contraseña
        confirmPasswordInput.addEventListener('input', () => {
            this.validatePasswordConfirmation(passwordInput, confirmPasswordInput);
        });

        // Validación de campos obligatorios
        [firstNameInput, lastNameInput, phoneInput].forEach(input => {
            input.addEventListener('blur', () => {
                this.validateRequired(input);
            });
        });

        // Validación de tipo de usuario
        userTypeInputs.forEach(input => {
            input.addEventListener('change', () => {
                this.validateUserType(userTypeInputs);
            });
        });

        // Validación antes de enviar
        form.addEventListener('submit', (e) => {
            if (!this.validateRegisterForm(form)) {
                e.preventDefault();
            }
        });
    }

    setupLoginValidation(form) {
        const emailInput = form.querySelector('#Email');
        const passwordInput = form.querySelector('#Password');

        // Validación de email en tiempo real
        emailInput.addEventListener('blur', () => {
            this.validateEmail(emailInput);
        });

        // Validación antes de enviar
        form.addEventListener('submit', (e) => {
            if (!this.validateLoginForm(form)) {
                e.preventDefault();
            }
        });
    }

    validateEmail(input) {
        const email = input.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        
        if (!email) {
            this.showError(input, 'El correo electrónico es obligatorio');
            return false;
        }
        
        if (!emailRegex.test(email)) {
            this.showError(input, 'Ingresa un correo electrónico válido');
            return false;
        }
        
        this.clearError(input);
        return true;
    }

    validatePassword(input) {
        const password = input.value;
        
        if (password.length < 6) {
            this.showError(input, 'La contraseña debe tener al menos 6 caracteres');
            return false;
        }
        
        this.clearError(input);
        return true;
    }

    validatePasswordConfirmation(passwordInput, confirmInput) {
        const password = passwordInput.value;
        const confirmPassword = confirmInput.value;
        
        if (confirmPassword && password !== confirmPassword) {
            this.showError(confirmInput, 'Las contraseñas no coinciden');
            return false;
        }
        
        if (confirmPassword) {
            this.clearError(confirmInput);
        }
        
        return true;
    }

    validateRequired(input) {
        const value = input.value.trim();
        const name = input.getAttribute('name');

        if (!value) {
            this.showError(input, 'Este campo es obligatorio');
            return false;
        }

        // Validación específica para nombre y apellido
        if (name === 'FirstName' || name === 'LastName') {
            const nameRegex = /^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$/;
            if (!nameRegex.test(value)) {
                this.showError(input, 'Solo se permiten letras y espacios');
                return false;
            }
        }

        // Validación específica para teléfono
        if (name === 'PhoneNumber') {
            const phoneRegex = /^\+?[0-9]{7,15}$/;
            if (!phoneRegex.test(value)) {
                this.showError(input, 'Solo se permiten numeros, minimo 7 digitos');
                return false;
            }
        }

        this.clearError(input);
        return true;
    }

    validateUserType(inputs) {
        const selected = Array.from(inputs).some(input => input.checked);
        
        if (!selected) {
            this.showError(inputs[0].closest('.form-group'), 'Debes seleccionar un tipo de usuario');
            return false;
        }
        
        this.clearError(inputs[0].closest('.form-group'));
        return true;
    }

    validateRegisterForm(form) {
        const emailInput = form.querySelector('#Email');
        const passwordInput = form.querySelector('#Password');
        const confirmPasswordInput = form.querySelector('#ConfirmPassword');
        const firstNameInput = form.querySelector('#FirstName');
        const lastNameInput = form.querySelector('#LastName');
        const phoneInput = form.querySelector('#PhoneNumber');
        const userTypeInputs = form.querySelectorAll('input[name="UserType"]');

        const validations = [
            this.validateEmail(emailInput),
            this.validatePassword(passwordInput),
            this.validatePasswordConfirmation(passwordInput, confirmPasswordInput),
            this.validateRequired(firstNameInput),
            this.validateRequired(lastNameInput),
            this.validateRequired(phoneInput),
            this.validateUserType(userTypeInputs)
        ];

        return validations.every(validation => validation);
    }

    validateLoginForm(form) {
        const emailInput = form.querySelector('#Email');
        const passwordInput = form.querySelector('#Password');

        return this.validateEmail(emailInput) && this.validateRequired(passwordInput);
    }

    showError(input, message) {
        this.clearError(input);
        
        const errorDiv = document.createElement('div');
        errorDiv.className = 'field-error';
        errorDiv.textContent = message;
        errorDiv.style.color = '#c0392b';
        errorDiv.style.fontSize = '0.85rem';
        errorDiv.style.marginTop = '0.3rem';
        
        input.parentNode.appendChild(errorDiv);
        input.style.borderColor = '#e57373';
    }

    clearError(input) {
        const errorDiv = input.parentNode.querySelector('.field-error');
        if (errorDiv) {
            errorDiv.remove();
        }
        input.style.borderColor = '#e0e0e0';
    }

    initializeLoadingStates() {
        const forms = document.querySelectorAll('.register-form, .login-form');
        
        forms.forEach(form => {
            const submitButton = form.querySelector('button[type="submit"]');
            const originalText = submitButton.textContent;
            
            form.addEventListener('submit', () => {
                submitButton.disabled = true;
                submitButton.textContent = 'Procesando...';
                submitButton.style.opacity = '0.7';
            });
        });
    }

    initializeFocusStates() {
        const inputs = document.querySelectorAll('input[type="text"], input[type="email"], input[type="password"], input[type="tel"]');
        
        inputs.forEach(input => {
            input.addEventListener('focus', () => {
                input.style.borderColor = '#2a3cff';
                input.style.boxShadow = '0 0 0 2px rgba(42, 60, 255, 0.1)';
            });
            
            input.addEventListener('blur', () => {
                input.style.boxShadow = 'none';
                if (!input.classList.contains('error')) {
                    input.style.borderColor = '#e0e0e0';
                }
            });
        });
    }
}

// Inicializar validación cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', () => {
    new AuthValidation();
}); 