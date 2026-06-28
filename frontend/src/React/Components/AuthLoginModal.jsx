import { useEffect, useRef, useState } from 'react';
import {
  changePassword,
  getUserByIdDisplay,
  loginUser,
  registerUser,
  saveAuthSession,
  sendOtp,
} from '../../../Services/MediaService.tsx';
import '../../CSS/Login_style.css';

const initialLoginForm = {
  idDisplay: '',
  password: '',
};

const initialRegisterForm = {
  email: '',
  displayName: '',
  idDisplay: '',
  password: '',
  confirmPassword: '',
  otpCode: '',
};

const initialChangePasswordForm = {
  email: '',
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
  otpCode: '',
};

const initialErrors = {};

export default function AuthLoginModal({
  isOpen,
  initialMode = 'login',
  currentUserEmail = '',
  onClose,
  onAuthenticated,
  reason,
}) {
  const [mode, setMode] = useState(initialMode);
  const [registerStep, setRegisterStep] = useState('form');
  const [changePasswordStep, setChangePasswordStep] = useState('form');
  const [loginForm, setLoginForm] = useState(initialLoginForm);
  const [registerForm, setRegisterForm] = useState(initialRegisterForm);
  const [changePasswordForm, setChangePasswordForm] = useState({
    ...initialChangePasswordForm,
    email: currentUserEmail,
  });
  const [fieldErrors, setFieldErrors] = useState(initialErrors);
  const [message, setMessage] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showLoginPassword, setShowLoginPassword] = useState(false);
  const [showRegisterPassword, setShowRegisterPassword] = useState(false);
  const [showRegisterConfirmPassword, setShowRegisterConfirmPassword] = useState(false);
  const [showChangeOldPassword, setShowChangeOldPassword] = useState(false);
  const [showChangeNewPassword, setShowChangeNewPassword] = useState(false);
  const [showChangeConfirmPassword, setShowChangeConfirmPassword] = useState(false);
  const [otpDigits, setOtpDigits] = useState(['', '', '', '', '', '']);

  // OTP countdown state
  const [otpCountdown, setOtpCountdown] = useState(90); // 90 seconds = 1 minute 30 seconds
  const [otpCountdownToken, setOtpCountdownToken] = useState(0);
  const countdownIntervalRef = useRef(null);

  const otpInputRefs = useRef([]); // Ref for OTP input fields

  const loginIdDisplayRef = useRef(null);
  const loginPasswordRef = useRef(null);
  const registerEmailRef = useRef(null);
  const registerDisplayNameRef = useRef(null);
  const registerIdDisplayRef = useRef(null);
  const registerPasswordRef = useRef(null);
  const registerConfirmPasswordRef = useRef(null);
  const registerOtpCodeRef = useRef(null);
  const changePasswordEmailRef = useRef(null);
  const changePasswordOldPasswordRef = useRef(null);
  const changePasswordNewPasswordRef = useRef(null);
  const changePasswordConfirmPasswordRef = useRef(null);
  const changePasswordOtpCodeRef = useRef(null);

  const isRegisterMode = mode === 'register';
  const isChangePasswordMode = mode === 'change-password';
  const isOtpStep = isRegisterMode && registerStep === 'otp';
  const isChangePasswordOtpStep = isChangePasswordMode && changePasswordStep === 'otp';

  const clearOtpCountdownInterval = () => {
    if (countdownIntervalRef.current) {
      clearInterval(countdownIntervalRef.current);
      countdownIntervalRef.current = null;
    }
  };

  const startOtpCountdown = () => {
    clearOtpCountdownInterval();
    setOtpCountdown(90);
    countdownIntervalRef.current = window.setInterval(() => {
      setOtpCountdown((prevCountdown) => {
        if (prevCountdown <= 1) {
          clearOtpCountdownInterval();
          return 0;
        }
        return prevCountdown - 1;
      });
    }, 1000);
  };

  const requestOtpCountdownRestart = () => {
    clearOtpCountdownInterval();
    setOtpCountdown(90);
    setOtpCountdownToken((currentToken) => currentToken + 1);
  };

  const switchMode = (nextMode) => {
    setMode(nextMode);
    setRegisterStep('form');
    setChangePasswordStep('form');
    setFieldErrors(initialErrors);
    setMessage('');
    setOtpDigits(['', '', '', '', '', '']);
    setOtpCountdown(90);
    clearOtpCountdownInterval();
    setOtpCountdownToken(0);
    setChangePasswordForm({
      ...initialChangePasswordForm,
      email: currentUserEmail || localStorage.getItem('user_email') || '',
    });
  };

  const focusFirstInvalid = (errors, fieldOrder) => {
    const firstInvalidField = fieldOrder.find((field) => errors[field]);
    const refMap = {
      loginIdDisplay: loginIdDisplayRef,
      loginPassword: loginPasswordRef,
      registerEmail: registerEmailRef,
      registerDisplayName: registerDisplayNameRef,
      registerIdDisplay: registerIdDisplayRef,
      registerPassword: registerPasswordRef,
      registerConfirmPassword: registerConfirmPasswordRef,
      registerOtpCode: registerOtpCodeRef,
      changePasswordEmail: changePasswordEmailRef,
      changePasswordOldPassword: changePasswordOldPasswordRef,
      changePasswordNewPassword: changePasswordNewPasswordRef,
      changePasswordConfirmPassword: changePasswordConfirmPasswordRef,
      changePasswordOtpCode: changePasswordOtpCodeRef,
    };
    const ref = firstInvalidField ? refMap[firstInvalidField] : null;
    ref?.current?.focus();
  };

  useEffect(() => {
    if (!isOpen || (!isOtpStep && !isChangePasswordOtpStep)) {
      clearOtpCountdownInterval();
      return undefined;
    }

    startOtpCountdown();

    return () => {
      clearOtpCountdownInterval();
    };
  }, [isOpen, isOtpStep, isChangePasswordOtpStep, otpCountdownToken]);

  useEffect(() => {
    if (!isOpen) return;

    const resolvedEmail = currentUserEmail || localStorage.getItem('user_email') || '';
    setChangePasswordForm((currentForm) => ({
      ...currentForm,
      email: resolvedEmail,
    }));
  }, [isOpen, currentUserEmail]);

  const formattedOtpCountdown = () => {
    const minutes = Math.floor(otpCountdown / 60);
    const seconds = String(otpCountdown % 60).padStart(2, '0');
    return `${minutes}:${seconds}`;
  };

  const validateLogin = () => {
    const errors = {};

    if (!loginForm.idDisplay.trim()) {
      errors.loginIdDisplay = 'Vui lòng nhập idname.';
    }

    if (!loginForm.password) {
      errors.loginPassword = 'Vui lòng nhập mật khẩu.';
    }

    setFieldErrors(errors);
    focusFirstInvalid(errors, ['loginIdDisplay', 'loginPassword']);

    return Object.keys(errors).length === 0;
  };

  const validateRegisterInfo = () => {
    const errors = {};

    if (!registerForm.email.trim()) {
      errors.registerEmail = 'Vui lòng nhập email.';
    }

    if (!registerForm.displayName.trim()) {
      errors.registerDisplayName = 'Vui lòng nhập tên hiển thị.';
    }

    if (!registerForm.idDisplay.trim()) {
      errors.registerIdDisplay = 'Vui lòng nhập idname.';
    }

    if (!registerForm.password) {
      errors.registerPassword = 'Vui lòng nhập mật khẩu.';
    }

    if (!registerForm.confirmPassword) {
      errors.registerConfirmPassword = 'Vui lòng nhập lại mật khẩu.';
    } else if (registerForm.password && registerForm.confirmPassword !== registerForm.password) {
      errors.registerConfirmPassword = 'Mật khẩu nhập lại chưa trùng khớp.';
    }

    setFieldErrors(errors);
    focusFirstInvalid(errors, [
      'registerEmail',
      'registerDisplayName',
      'registerIdDisplay',
      'registerPassword',
      'registerConfirmPassword',
    ]);

    return Object.keys(errors).length === 0;
  };

  const validateRegisterOtp = () => {
    const errors = {};

    if (!registerForm.otpCode.trim()) {
      errors.registerOtpCode = 'Vui lòng nhập mã OTP.';
    } else if (!/^\d{6}$/.test(registerForm.otpCode.trim())) {
      errors.registerOtpCode = 'Mã OTP phải gồm 6 số.';
    }

    setFieldErrors(errors);
    if (errors.registerOtpCode) {
      registerOtpCodeRef.current?.focus();
    }

    return Object.keys(errors).length === 0;
  };

  const validateChangePasswordInfo = () => {
    const errors = {};

    if (!changePasswordForm.email.trim()) {
      errors.changePasswordEmail = 'Không tìm thấy email tài khoản hiện tại.';
    }

    if (!changePasswordForm.oldPassword) {
      errors.changePasswordOldPassword = 'Vui lòng nhập mật khẩu cũ.';
    }

    if (!changePasswordForm.newPassword) {
      errors.changePasswordNewPassword = 'Vui lòng nhập mật khẩu mới.';
    }

    if (!changePasswordForm.confirmPassword) {
      errors.changePasswordConfirmPassword = 'Vui lòng nhập lại mật khẩu mới.';
    } else if (changePasswordForm.newPassword && changePasswordForm.confirmPassword !== changePasswordForm.newPassword) {
      errors.changePasswordConfirmPassword = 'Mật khẩu nhập lại chưa trùng khớp.';
    }

    setFieldErrors(errors);
    focusFirstInvalid(errors, [
      'changePasswordEmail',
      'changePasswordOldPassword',
      'changePasswordNewPassword',
      'changePasswordConfirmPassword',
    ]);

    return Object.keys(errors).length === 0;
  };

  const validateChangePasswordOtp = () => {
    const errors = {};

    if (!changePasswordForm.otpCode.trim()) {
      errors.changePasswordOtpCode = 'Vui lòng nhập mã OTP.';
    } else if (!/^\d{6}$/.test(changePasswordForm.otpCode.trim())) {
      errors.changePasswordOtpCode = 'Mã OTP phải gồm 6 số.';
    }

    setFieldErrors(errors);
    if (errors.changePasswordOtpCode) {
      changePasswordOtpCodeRef.current?.focus();
    }

    return Object.keys(errors).length === 0;
  };

  const clearFieldError = (field) => {
    setFieldErrors((currentErrors) => {
      if (!currentErrors[field]) return currentErrors;

      const nextErrors = { ...currentErrors };
      delete nextErrors[field];
      return nextErrors;
    });
  };

  const ensureRegisterIdDisplayAvailable = async () => {
    const idDisplay = registerForm.idDisplay.trim();
    if (!idDisplay) return false;

    try {
      const existingUser = await getUserByIdDisplay(idDisplay);
      if (existingUser) {
        setFieldErrors((currentErrors) => ({
          ...currentErrors,
          registerIdDisplay: `Tên người dùng '${idDisplay}' đã tồn tại.`,
        }));
        registerIdDisplayRef.current?.focus();
        return false;
      }

      return true;
    } catch (error) {
      setMessage(error?.message || 'Không thể kiểm tra idname lúc này.');
      return false;
    }
  };

  const handleLoginSubmit = async (event) => {
    event.preventDefault();

    if (!validateLogin()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      const auth = await loginUser(loginForm.idDisplay.trim(), loginForm.password);
      saveAuthSession(auth);
      setLoginForm(initialLoginForm);
      onAuthenticated?.(auth);
    } catch (error) {
      setMessage(error.message || 'Đăng nhập thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSendOtp = async (event) => {
    event.preventDefault();

    if (!validateRegisterInfo()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      const isIdDisplayAvailable = await ensureRegisterIdDisplayAvailable();
      if (!isIdDisplayAvailable) {
        return;
      }

      await sendOtp(registerForm.email.trim(), 'register');
      setOtpDigits(['', '', '', '', '', '']);
      setRegisterForm((currentForm) => ({ ...currentForm, otpCode: '' }));
      setRegisterStep('otp');
      requestOtpCountdownRestart();
      setFieldErrors(initialErrors);
      setMessage('OTP đã gửi. Vui lòng nhập mã xác nhận để hoàn tất đăng ký.');
    } catch (error) {
      setMessage(error.message || 'Gửi OTP thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleResendOtp = async (event) => {
    event.preventDefault();

    if (!validateRegisterInfo()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      const isIdDisplayAvailable = await ensureRegisterIdDisplayAvailable();
      if (!isIdDisplayAvailable) {
        return;
      }

      await sendOtp(registerForm.email.trim(), 'register');
      setOtpDigits(['', '', '', '', '', '']);
      setRegisterForm((currentForm) => ({ ...currentForm, otpCode: '' }));
      requestOtpCountdownRestart();
      setMessage('OTP mới đã được gửi. Vui lòng nhập mã xác nhận để hoàn tất đăng ký.');
    } catch (error) {
      setMessage(error.message || 'Gửi OTP mới thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRegisterSubmit = async (event) => {
    event.preventDefault();

    if (!validateRegisterOtp()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      const auth = await registerUser({
        email: registerForm.email.trim(),
        otpCode: registerForm.otpCode.trim(),
        idDisplay: registerForm.idDisplay.trim(),
        displayName: registerForm.displayName.trim(),
        password: registerForm.password,
      });

      saveAuthSession(auth);
      setRegisterForm(initialRegisterForm);
      setOtpDigits(['', '', '', '', '', '']);
      setRegisterStep('form');
      onAuthenticated?.(auth);
    } catch (error) {
      setMessage(error.message || 'Đăng ký thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSendChangePasswordOtp = async (event) => {
    event.preventDefault();

    if (!validateChangePasswordInfo()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      await sendOtp(changePasswordForm.email.trim(), 'change_password');
      setOtpDigits(['', '', '', '', '', '']);
      setChangePasswordForm((currentForm) => ({ ...currentForm, otpCode: '' }));
      setChangePasswordStep('otp');
      requestOtpCountdownRestart();
      setFieldErrors(initialErrors);
      setMessage('OTP đã gửi. Vui lòng nhập mã xác nhận để đổi mật khẩu.');
    } catch (error) {
      setMessage(error.message || 'Gửi OTP thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleResendChangePasswordOtp = async (event) => {
    event.preventDefault();

    if (!validateChangePasswordInfo()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      await sendOtp(changePasswordForm.email.trim(), 'change_password');
      setOtpDigits(['', '', '', '', '', '']);
      setChangePasswordForm((currentForm) => ({ ...currentForm, otpCode: '' }));
      requestOtpCountdownRestart();
      setMessage('OTP mới đã được gửi. Vui lòng nhập mã xác nhận để đổi mật khẩu.');
    } catch (error) {
      setMessage(error.message || 'Gửi OTP mới thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChangePasswordSubmit = async (event) => {
    event.preventDefault();

    if (!validateChangePasswordOtp()) {
      setMessage('');
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      await changePassword({
        email: changePasswordForm.email.trim(),
        oldPassword: changePasswordForm.oldPassword,
        otpCode: changePasswordForm.otpCode.trim(),
        newPassword: changePasswordForm.newPassword,
      });

      setChangePasswordForm(initialChangePasswordForm);
      setChangePasswordStep('form');
      setOtpDigits(['', '', '', '', '', '']);
      setOtpCountdown(90);
      onClose?.();
    } catch (error) {
      setMessage(error.message || 'Đổi mật khẩu thất bại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const updateLoginField = (field, value) => {
    setLoginForm((currentForm) => ({ ...currentForm, [field]: value }));
    clearFieldError(field === 'idDisplay' ? 'loginIdDisplay' : 'loginPassword');
    setMessage('');
  };

  const updateRegisterField = (field, value) => {
    setRegisterForm((currentForm) => ({ ...currentForm, [field]: value }));
    const errorFieldByFormField = {
      email: 'registerEmail',
      displayName: 'registerDisplayName',
      idDisplay: 'registerIdDisplay',
      password: 'registerPassword',
      confirmPassword: 'registerConfirmPassword',
      otpCode: 'registerOtpCode',
    };

    clearFieldError(errorFieldByFormField[field]);

    if (field === 'password' || field === 'confirmPassword') {
      clearFieldError('registerConfirmPassword');
    }

    setMessage('');
  };

  const updateChangePasswordField = (field, value) => {
    setChangePasswordForm((currentForm) => ({ ...currentForm, [field]: value }));
    const errorFieldByFormField = {
      email: 'changePasswordEmail',
      oldPassword: 'changePasswordOldPassword',
      newPassword: 'changePasswordNewPassword',
      confirmPassword: 'changePasswordConfirmPassword',
      otpCode: 'changePasswordOtpCode',
    };

    clearFieldError(errorFieldByFormField[field]);

    if (field === 'newPassword' || field === 'confirmPassword') {
      clearFieldError('changePasswordConfirmPassword');
    }

    setMessage('');
  };

  if (!isOpen) return null;

  const fieldClassName = (errorKey) => `auth-field ${fieldErrors[errorKey] ? 'has-error' : ''}`;

  const renderFieldError = (errorKey) => (
    fieldErrors[errorKey] ? <small className="auth-field-error">{fieldErrors[errorKey]}</small> : null
  );

  const title = isRegisterMode
    ? (isOtpStep ? 'Xác nhận OTP' : 'Đăng ký')
    : isChangePasswordMode
      ? (isChangePasswordOtpStep ? 'Xác nhận OTP đổi mật khẩu' : 'Đổi mật khẩu')
      : 'Đăng nhập';
  const subtitle = isRegisterMode
    ? isOtpStep
      ? `Nhập mã OTP đã gửi đến ${registerForm.email}.`
      : 'Tạo tài khoản TuneVault mới.'
    : isChangePasswordMode
      ? isChangePasswordOtpStep
        ? `Nhập mã OTP đã gửi đến ${changePasswordForm.email}.`
        : 'Đổi mật khẩu cho tài khoản hiện tại.'
      : reason || 'Bạn cần đăng nhập để tiếp tục thao tác này.';

  const otpButtonText = isSubmitting
    ? 'Đang xử lý...'
    : (isOtpStep || isChangePasswordOtpStep)
      ? (otpCountdown > 0 ? `Xác nhận (${formattedOtpCountdown()})` : 'Gửi lại mã')
      : 'Gửi mã OTP';

  const handleOtpPrimaryAction = async (event) => {
    if (isChangePasswordMode) {
      if (!isChangePasswordOtpStep) {
        await handleSendChangePasswordOtp(event);
        return;
      }

      if (otpCountdown > 0) {
        await handleChangePasswordSubmit(event);
        return;
      }

      await handleResendChangePasswordOtp(event);
      return;
    }

    if (!isOtpStep) {
      await handleSendOtp(event);
      return;
    }

    if (otpCountdown > 0) {
      await handleRegisterSubmit(event);
      return;
    }

    await handleResendOtp(event);
  };

  const handleOtpInputChange = (index, value) => {
    const newOtpDigits = [...otpDigits];
    const input = value.slice(-1);
    if (!/^\d*$/.test(input)) return;

    newOtpDigits[index] = input;
    setOtpDigits(newOtpDigits);
    if (isChangePasswordMode) {
      updateChangePasswordField('otpCode', newOtpDigits.join(''));
    } else {
      updateRegisterField('otpCode', newOtpDigits.join(''));
    }

    if (input && index < otpDigits.length - 1) {
      otpInputRefs.current[index + 1]?.focus();
    }
  };

  const handleOtpInputKeyDown = (e, index) => {
    if (e.key === 'Backspace') {
      if (!otpDigits[index] && index > 0) {
        // If current field is empty, move to previous and clear it
        otpInputRefs.current[index - 1]?.focus();
        const newOtpDigits = [...otpDigits];
        newOtpDigits[index - 1] = '';
        setOtpDigits(newOtpDigits);
        if (isChangePasswordMode) {
          updateChangePasswordField('otpCode', newOtpDigits.join(''));
        } else {
          updateRegisterField('otpCode', newOtpDigits.join(''));
        }
      }
    } else if (e.key === 'ArrowLeft') {
      if (index > 0) otpInputRefs.current[index - 1]?.focus();
    } else if (e.key === 'ArrowRight') {
      if (index < otpDigits.length - 1) otpInputRefs.current[index + 1]?.focus();
    }
  };

  const handleOtpPaste = (e) => {
    e.preventDefault();
    const pasteData = e.clipboardData.getData('text').trim();
    if (/^\d{6}$/.test(pasteData)) {
      const newOtpDigits = pasteData.split('');
      setOtpDigits(newOtpDigits);
      if (isChangePasswordMode) {
        updateChangePasswordField('otpCode', newOtpDigits.join(''));
      } else {
        updateRegisterField('otpCode', newOtpDigits.join(''));
      }
      otpInputRefs.current[otpDigits.length - 1]?.focus();
    }
  };

  return (
    <div className="auth-modal-overlay" role="presentation" onMouseDown={onClose}>
      <section
        className="auth-card auth-modal-card"
        role="dialog"
        aria-modal="true"
        aria-labelledby="auth-modal-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <button className="auth-modal-close" type="button" aria-label="Đóng đăng nhập" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>

        <div className="auth-card-heading">
          <h1 id="auth-modal-title">{title}</h1>
          <p>{subtitle}</p>
        </div>

        {isRegisterMode ? (
          <form className="auth-form" noValidate onSubmit={handleOtpPrimaryAction}>
            {isOtpStep ? (
              <div className="auth-otp-panel">
                <button className="auth-back" type="button" onClick={() => setRegisterStep('form')}>
                  <span className="material-symbols-outlined">arrow_back</span>
                  <span>Sửa thông tin đăng ký</span>
                </button>

                <label className={fieldClassName('registerOtpCode')}>
                  <span>OTP CODE</span>
                  <div className="otp-input-container">
                    {otpDigits.map((digit, index) => (
                      <input
                        key={index}
                        ref={(el) => {
                          otpInputRefs.current[index] = el;
                          if (index === 0) {
                            registerOtpCodeRef.current = el;
                          }
                        }}
                        type="text"
                        inputMode="numeric"
                        maxLength={1}
                        value={digit}
                        onChange={(e) => handleOtpInputChange(index, e.target.value)}
                        onKeyDown={(e) => handleOtpInputKeyDown(e, index)} // Add keydown handler
                        onPaste={handleOtpPaste}
                        className="otp-input-field"
                        autoFocus={index === 0} // Auto-focus the first field
                        disabled={isSubmitting}
                      />
                    ))}
                  </div>
                  {renderFieldError('registerOtpCode')}
                </label>
              </div>
            ) : (
              <>
                <label className={fieldClassName('registerEmail')}>
                  <span>Email</span>
                  <input
                    autoComplete="email"
                    autoFocus
                    onChange={(event) => updateRegisterField('email', event.target.value)}
                    placeholder="name@example.com"
                    ref={registerEmailRef}
                    type="email"
                    value={registerForm.email}
                  />
                  {renderFieldError('registerEmail')}
                </label>

                <label className={fieldClassName('registerDisplayName')}>
                  <span>Tên hiển thị</span>
                  <input
                    autoComplete="name"
                    onChange={(event) => updateRegisterField('displayName', event.target.value)}
                    placeholder="Nguyen Thanh Long"
                    ref={registerDisplayNameRef}
                    type="text"
                    value={registerForm.displayName}
                  />
                  {renderFieldError('registerDisplayName')}
                </label>

                <label className={fieldClassName('registerIdDisplay')}>
                  <span>Idname</span>
                  <input
                    autoComplete="username"
                    onChange={(event) => updateRegisterField('idDisplay', event.target.value)}
                    placeholder="long_music"
                    ref={registerIdDisplayRef}
                    type="text"
                    value={registerForm.idDisplay}
                  />
                  {renderFieldError('registerIdDisplay')}
                </label>

                <label className={fieldClassName('registerPassword')}>
                  <span>Password</span>
                  <div className="auth-password-control">
                    <input
                      autoComplete="new-password"
                      onChange={(event) => updateRegisterField('password', event.target.value)}
                      placeholder="••••••••"
                      ref={registerPasswordRef}
                      type={showRegisterPassword ? 'text' : 'password'}
                      value={registerForm.password}
                    />
                    <button
                      type="button"
                      aria-label={showRegisterPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                      onClick={() => setShowRegisterPassword((isVisible) => !isVisible)}
                    >
                      <span className="material-symbols-outlined">
                        {showRegisterPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {renderFieldError('registerPassword')}
                </label>

                <label className={fieldClassName('registerConfirmPassword')}>
                  <span>Nhập lại password</span>
                  <div className="auth-password-control">
                    <input
                      autoComplete="new-password"
                      onChange={(event) => updateRegisterField('confirmPassword', event.target.value)}
                      placeholder="••••••••"
                      ref={registerConfirmPasswordRef}
                      type={showRegisterConfirmPassword ? 'text' : 'password'}
                      value={registerForm.confirmPassword}
                    />
                    <button
                      type="button"
                      aria-label={showRegisterConfirmPassword ? 'Ẩn mật khẩu nhập lại' : 'Hiện mật khẩu nhập lại'}
                      onClick={() => setShowRegisterConfirmPassword((isVisible) => !isVisible)}
                    >
                      <span className="material-symbols-outlined">
                        {showRegisterConfirmPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {renderFieldError('registerConfirmPassword')}
                </label>
              </>
            )}

            {message && <p className={message.includes('OTP đã gửi') ? 'auth-message success' : 'auth-message'}>{message}</p>}

            <button
              className="auth-submit"
              type="submit"
              disabled={isSubmitting}
            >
              {otpButtonText}
            </button>

            {!isOtpStep && (
              <div className="auth-switch">
                <span>Đã có tài khoản?</span>
                <button type="button" onClick={() => switchMode('login')}>Đăng nhập</button>
              </div>
            )}
          </form>
        ) : isChangePasswordMode ? (
          <form className="auth-form" noValidate onSubmit={handleOtpPrimaryAction}>
            {isChangePasswordOtpStep ? (
              <div className="auth-otp-panel">
                <button className="auth-back" type="button" onClick={() => setChangePasswordStep('form')}>
                  <span className="material-symbols-outlined">arrow_back</span>
                  <span>Sửa thông tin đổi mật khẩu</span>
                </button>

                <label className={fieldClassName('changePasswordOtpCode')}>
                  <span>OTP CODE</span>
                  <div className="otp-input-container">
                    {otpDigits.map((digit, index) => (
                      <input
                        key={index}
                        ref={(el) => {
                          otpInputRefs.current[index] = el;
                          if (index === 0) {
                            changePasswordOtpCodeRef.current = el;
                          }
                        }}
                        type="text"
                        inputMode="numeric"
                        maxLength={1}
                        value={digit}
                        onChange={(e) => handleOtpInputChange(index, e.target.value)}
                        onKeyDown={(e) => handleOtpInputKeyDown(e, index)}
                        onPaste={handleOtpPaste}
                        className="otp-input-field"
                        autoFocus={index === 0}
                        disabled={isSubmitting}
                      />
                    ))}
                  </div>
                  {renderFieldError('changePasswordOtpCode')}
                </label>
              </div>
            ) : (
              <>
                <label className={fieldClassName('changePasswordEmail')}>
                  <span>Email</span>
                  <input
                    autoComplete="email"
                    autoFocus
                    onChange={(event) => updateChangePasswordField('email', event.target.value)}
                    placeholder="name@example.com"
                    ref={changePasswordEmailRef}
                    type="email"
                    value={changePasswordForm.email}
                    readOnly
                  />
                  {renderFieldError('changePasswordEmail')}
                </label>

                <label className={fieldClassName('changePasswordOldPassword')}>
                  <span>Password cũ</span>
                  <div className="auth-password-control">
                    <input
                      autoComplete="current-password"
                      onChange={(event) => updateChangePasswordField('oldPassword', event.target.value)}
                      placeholder="••••••••"
                      ref={changePasswordOldPasswordRef}
                      type={showChangeOldPassword ? 'text' : 'password'}
                      value={changePasswordForm.oldPassword}
                    />
                    <button
                      type="button"
                      aria-label={showChangeOldPassword ? 'Ẩn mật khẩu cũ' : 'Hiện mật khẩu cũ'}
                      onClick={() => setShowChangeOldPassword((isVisible) => !isVisible)}
                    >
                      <span className="material-symbols-outlined">
                        {showChangeOldPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {renderFieldError('changePasswordOldPassword')}
                </label>

                <label className={fieldClassName('changePasswordNewPassword')}>
                  <span>Password mới</span>
                  <div className="auth-password-control">
                    <input
                      autoComplete="new-password"
                      onChange={(event) => updateChangePasswordField('newPassword', event.target.value)}
                      placeholder="••••••••"
                      ref={changePasswordNewPasswordRef}
                      type={showChangeNewPassword ? 'text' : 'password'}
                      value={changePasswordForm.newPassword}
                    />
                    <button
                      type="button"
                      aria-label={showChangeNewPassword ? 'Ẩn mật khẩu mới' : 'Hiện mật khẩu mới'}
                      onClick={() => setShowChangeNewPassword((isVisible) => !isVisible)}
                    >
                      <span className="material-symbols-outlined">
                        {showChangeNewPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {renderFieldError('changePasswordNewPassword')}
                </label>

                <label className={fieldClassName('changePasswordConfirmPassword')}>
                  <span>Nhập lại password mới</span>
                  <div className="auth-password-control">
                    <input
                      autoComplete="new-password"
                      onChange={(event) => updateChangePasswordField('confirmPassword', event.target.value)}
                      placeholder="••••••••"
                      ref={changePasswordConfirmPasswordRef}
                      type={showChangeConfirmPassword ? 'text' : 'password'}
                      value={changePasswordForm.confirmPassword}
                    />
                    <button
                      type="button"
                      aria-label={showChangeConfirmPassword ? 'Ẩn mật khẩu nhập lại' : 'Hiện mật khẩu nhập lại'}
                      onClick={() => setShowChangeConfirmPassword((isVisible) => !isVisible)}
                    >
                      <span className="material-symbols-outlined">
                        {showChangeConfirmPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                  {renderFieldError('changePasswordConfirmPassword')}
                </label>
              </>
            )}

            {message && <p className={message.includes('OTP đã gửi') ? 'auth-message success' : 'auth-message'}>{message}</p>}

            <button
              className="auth-submit"
              type="submit"
              disabled={isSubmitting}
            >
              {otpButtonText}
            </button>

            {!isChangePasswordOtpStep && (
              <div className="auth-switch">
                <span>Không muốn đổi mật khẩu?</span>
                <button type="button" onClick={onClose}>Đóng</button>
              </div>
            )}
          </form>
        ) : (
          <form className="auth-form" noValidate onSubmit={handleLoginSubmit}>
            <label className={fieldClassName('loginIdDisplay')}>
              <span>Idname</span>
              <input
                autoComplete="username"
                autoFocus
                onChange={(event) => updateLoginField('idDisplay', event.target.value)}
                placeholder="long_music"
                ref={loginIdDisplayRef}
                type="text"
                value={loginForm.idDisplay}
              />
              {renderFieldError('loginIdDisplay')}
            </label>

            <label className={fieldClassName('loginPassword')}>
              <span>Password</span>
              <div className="auth-password-control">
                <input
                  autoComplete="current-password"
                  onChange={(event) => updateLoginField('password', event.target.value)}
                  placeholder="••••••••"
                  ref={loginPasswordRef}
                  type={showLoginPassword ? 'text' : 'password'}
                  value={loginForm.password}
                />
                <button
                  type="button"
                  aria-label={showLoginPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                  onClick={() => setShowLoginPassword((isVisible) => !isVisible)}
                >
                  <span className="material-symbols-outlined">
                    {showLoginPassword ? 'visibility_off' : 'visibility'}
                  </span>
                </button>
              </div>
              {renderFieldError('loginPassword')}
            </label>

            {message && <p className="auth-message">{message}</p>}

            <button className="auth-submit" type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Đang đăng nhập...' : 'Đăng nhập'}
            </button>

            <div className="auth-switch">
              <span>Chưa có tài khoản?</span>
              <button type="button" onClick={() => switchMode('register')}>Đăng ký</button>
            </div>
          </form>
        )}
      </section>
    </div>
  );
}
