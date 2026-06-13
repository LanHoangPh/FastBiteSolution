/**
 * Utility functions for validating authentication fields and forms.
 * Returns i18n translation keys if validation fails, or null/empty if valid.
 */

export const validators = {
  email: (email: string): string | null => {
    if (!email || !email.trim()) {
      return "auth.validation.emailRequired";
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email.trim())) {
      return "auth.validation.invalidEmail";
    }
    return null;
  },

  password: (password: string): string | null => {
    if (!password) {
      return "auth.validation.passwordRequired";
    }
    if (password.length < 6) {
      return "auth.validation.passwordTooShort";
    }
    return null;
  },

  firstName: (name: string): string | null => {
    if (!name || !name.trim()) {
      return "auth.validation.firstNameRequired";
    }
    return null;
  },

  lastName: (name: string): string | null => {
    if (!name || !name.trim()) {
      return "auth.validation.lastNameRequired";
    }
    return null;
  },

  dateOfBirth: (dob: string): string | null => {
    if (!dob) {
      return "auth.validation.dobRequired";
    }
    const dobDate = new Date(dob);
    if (isNaN(dobDate.getTime())) {
      return "auth.validation.dobInvalid";
    }
    const today = new Date();
    if (dobDate >= today) {
      return "auth.validation.dobInvalid";
    }
    
    // Check if user is at least 13 years old
    let age = today.getFullYear() - dobDate.getFullYear();
    const monthDiff = today.getMonth() - dobDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dobDate.getDate())) {
      age--;
    }
    if (age < 13) {
      return "auth.validation.dobUnderage";
    }
    return null;
  },

  confirmPassword: (password: string, confirm: string): string | null => {
    if (!confirm) {
      return "auth.validation.confirmPasswordRequired";
    }
    if (password !== confirm) {
      return "auth.validation.passwordsDoNotMatch";
    }
    return null;
  },

  otp: (otp: string): string | null => {
    if (!otp || !otp.trim()) {
      return "auth.validation.otpRequired";
    }
    if (otp.trim().length !== 6) {
      return "auth.validation.invalidOtpLength";
    }
    return null;
  },

  token: (token: string): string | null => {
    if (!token || !token.trim()) {
      return "auth.validation.tokenRequired";
    }
    return null;
  }
};

export function validateLoginForm(email: string, password: string) {
  const emailError = validators.email(email);
  const passwordError = validators.password(password);

  const errors = {
    email: emailError || undefined,
    password: passwordError || undefined,
  };

  const isValid = !emailError && !passwordError;

  return { errors, isValid };
}

export function validateRegisterForm(fields: {
  firstName: string;
  lastName: string;
  email: string;
  dayOfBirth: string;
  password: string;
  confirmPassword: string;
}) {
  const firstNameError = validators.firstName(fields.firstName);
  const lastNameError = validators.lastName(fields.lastName);
  const emailError = validators.email(fields.email);
  const dobError = validators.dateOfBirth(fields.dayOfBirth);
  const passwordError = validators.password(fields.password);
  const confirmError = validators.confirmPassword(fields.password, fields.confirmPassword);

  const errors = {
    firstName: firstNameError || undefined,
    lastName: lastNameError || undefined,
    email: emailError || undefined,
    dayOfBirth: dobError || undefined,
    password: passwordError || undefined,
    confirmPassword: confirmError || undefined,
  };

  const isValid = !firstNameError && !lastNameError && !emailError && !dobError && !passwordError && !confirmError;

  return { errors, isValid };
}

export function validateForgotPasswordForm(email: string) {
  const emailError = validators.email(email);
  return {
    errors: { email: emailError || undefined },
    isValid: !emailError
  };
}

export function validateResetPasswordForm(fields: {
  otp: string;
  password: string;
  confirmPassword: string;
}) {
  const otpError = validators.otp(fields.otp);
  const passwordError = validators.password(fields.password);
  const confirmError = validators.confirmPassword(fields.password, fields.confirmPassword);

  const errors = {
    otp: otpError || undefined,
    password: passwordError || undefined,
    confirmPassword: confirmError || undefined,
  };

  const isValid = !otpError && !passwordError && !confirmError;

  return { errors, isValid };
}

export function validateVerifyEmailForm(token: string) {
  const tokenError = validators.token(token);
  return {
    errors: { token: tokenError || undefined },
    isValid: !tokenError
  };
}
