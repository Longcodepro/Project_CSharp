 import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import '../CSS/Signup_style.css';
function Signup()
{
    const navigate=useNavigate();
    const [phone, setPhone] = useState('');
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [pasteError, setPasteError] = useState(false);

    const isMismatch = confirmPassword !== '' && password !== confirmPassword;

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!isMismatch && password !== '') {
           navigate('/otp', { state: { email: phone } });
        }
    };

    const handlePaste = (e) => {
        e.preventDefault();
        setPasteError(true); // Bật thông báo lỗi dán
    };


    return(
        <div className="signup-page">
            <header className="signup-header">
                <img 
                src="https://storage.googleapis.com/pr-newsroom-wp/1/2018/11/Spotify_Logo_RGB_White.png"
                alt="spotify logo"
                className="signup-logo"
                />
            </header>

            <div className="signup-container">
                <h1 className="signup-title">Đăng ký để bắt đầu nghe</h1>

                <form className="signup-form" onSubmit={handleSubmit}>
                    <div className="input-group">
                        <label htmlFor='phone'>Số điện thoại của bạn</label>
                        <input 
                            type="tel" 
                            id="phone" 
                            placeholder="Nhập SĐT của bạn"
                            value={phone}
                            onChange={(e) => setPhone(e.target.value)}
                            required
                        />
                    </div>
                    <div className="input-group">
                        <label htmlFor='username'>Tên của bạn là gì?</label>
                        <input 
                            type="text" 
                            id="username" 
                            placeholder="Nhập tên người dùng"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            required
                        />
                        <p className="input-help">Tên này sẽ xuất hiện trên hồ sơ của bạn.</p>
                    </div>
                    <div className="input-group">
                        <label htmlFor='password'>Tạo mật khẩu</label>
                        <div className="password-wrapper">
                            <input 
                                type={showPassword ? "text" : "password"} 
                                id="password" 
                                placeholder="Nhập mật khẩu"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                            />
                            <button 
                                type="button" 
                                className="toggle-password"
                                onClick={() => setShowPassword(!showPassword)}
                            >
                                <i className={`fa-solid ${showPassword ? 'fa-eye-slash' : 'fa-eye'}`}></i>
                            </button>
                        </div>
                    </div>
                    <div className="input-group">
                        <label htmlFor='confirmPassword'>Xác nhận mật khẩu</label>
                        <input 
                            type="password" 
                            id="confirmPassword" 
                            placeholder="Nhập lại mật khẩu"
                            className={isMismatch ? "input-error" : ""}
                            value={confirmPassword}
                           onChange={(e) => {
                                setConfirmPassword(e.target.value);
                                if (pasteError) setPasteError(false); // Tắt lỗi dán khi người dùng tự gõ
                            }}
                            onPaste={handlePaste} // Chặn Ctrl + V
                            required
                        />
                        {/* Thông báo lỗi khi mật khẩu không khớp */}
                        {pasteError && (
                            <p className="error-text">
                                <i className="fa-solid fa-circle-exclamation"></i> Để bảo mật, vui lòng tự gõ lại mật khẩu thay vì sao chép.
                            </p>
                        )}
                        {isMismatch && (
                            <p className="error-text">
                                <i className="fa-solid fa-circle-exclamation"></i> Mật khẩu không trùng khớp.
                            </p>
                        )}
                    </div>
                    <button type="submit" className="btn-submit">Tiếp tục</button>
                </form>

                <hr className="signup-divider"/>

                <div className="otherSignup">
                    <button className="btn-other"  onClick={()=>navigate('/Signup')}>
                        <i className="fa-solid fa-envelope" ></i>
                        Đăng ký bằng Email
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-facebook"></i> Đăng ký với Facebook
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-apple"></i> Đăng ký với Apple
                    </button>
                </div>

                <hr className="signup-divider"/>

                <div className="signup-prompt">
                    <p>Bạn đã có tài khoản?</p>
                    <button className="btn-login-redirect" onClick={()=>navigate('/login')}>Đăng nhập</button>
                </div>
            </div>
        </div>
    );
}
export default Signup;