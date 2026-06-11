import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import '../CSS/Signup_style.css';

function OTP() {
    const navigate = useNavigate();
    const location = useLocation();
    
    // Lấy email từ trang Signup truyền sang, nếu không có sẽ hiển thị mặc định
    const email = location.state?.email || "email của bạn"; 
    
    const [otp, setOtp] = useState(new Array(6).fill(""));

    const handleChange = (element, index) => {
        const value = element.value;
        if (isNaN(value)) return; // Chỉ chấp nhận ký tự số

        const newOtp = [...otp];
        newOtp[index] = value.substring(value.length - 1); // Chỉ lấy số cuối cùng vừa nhập
        setOtp(newOtp);

        // Tự động chuyển tiêu điểm sang ô tiếp theo
        if (value && element.nextSibling) {
            element.nextSibling.focus();
        }
    };

    const handleKeyDown = (e, index) => {
        // Nhấn Backspace ở ô trống sẽ tự lùi về ô trước đó
        if (e.key === "Backspace" && !otp[index] && e.target.previousSibling) {
            e.target.previousSibling.focus();
        }
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        const otpCode = otp.join("");
        
        if (otpCode.length === 6) {
            alert(`Xác thực thành công với mã OTP: ${otpCode}`);
            // Thực hiện tiếp logic tạo tài khoản hoặc chuyển hướng về trang chủ/login tại đây
            // navigate('/login');
        } else {
            alert("Vui lòng nhập đầy đủ cả 6 chữ số OTP.");
        }
    };

    return (
        <div className="signup-page">
            <header className="signup-header">
                <img 
                    src="https://storage.googleapis.com/pr-newsroom-wp/1/2018/11/Spotify_Logo_RGB_White.png"
                    alt="spotify logo"
                    className="signup-logo"
                />
            </header>

            <div className="signup-container otp-container">
                <h1 className="signup-title">Xác thực tài khoản</h1>
                <p className="otp-instructions">
                    Vui lòng nhập mã gồm 6 chữ số vừa được gửi đến <br/> 
                    <strong>{email}</strong>
                </p>

                <form onSubmit={handleSubmit}>
                    {/* Khu vực 6 ô nhập mã OTP */}
                    <div className="otp-input-group">
                        {otp.map((data, index) => (
                            <input
                                key={index}
                                type="text"
                                className="otp-input"
                                maxLength="1"
                                value={data}
                                onChange={e => handleChange(e.target, index)}
                                onKeyDown={e => handleKeyDown(e, index)}
                                onFocus={e => e.target.select()} // Tự động bôi đen khi click vào ô
                            />
                        ))}
                    </div>

                    <button type="submit" className="btn-submit">Xác nhận</button>
                </form>

                <div className="otp-prompt">
                    <p>Bạn chưa nhận được mã xác thực?</p>
                    <button 
                        className="btn-login-redirect" 
                        onClick={() => {
                            setOtp(new Array(6).fill(""));
                            alert("Đã gửi lại mã OTP mới!");
                        }}
                    >
                        Gửi lại mã
                    </button>
                </div>
            </div>
        </div>
    );
}

export default OTP;