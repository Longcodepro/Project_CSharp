import React from 'react';
import {useNavigate} from 'react-router-dom';
import '../CSS/Login_style.css';
function Login()
{
    const navigate=useNavigate();
    return(
        <div className="login-page">
            <header className="login-header">
                <img 
                    src="https://storage.googleapis.com/pr-newsroom-wp/1/2018/11/Spotify_Logo_RGB_White.png"
                    alt="spotify Logo"
                    className="login-logo"
                  
                />
            </header>

            <div className="login-container">
                <h1 className="login-title">Chào mừng bạn quay trở lại</h1>

                <form className="login-form">
                    <div className="input-group">
                        <label htmlFor='email'>Email</label>
                        <input type="text" id="email"/>
                    </div>
                    <button type="button" className="btn-submit">Tiếp tục</button>
                </form>

                <hr className="login-divider"/>

                <div className="otherLogin">
                    <button className="btn-other">
                        <i className="fa-solid fa-mobile-screen"></i>
                        Tiếp tục với SĐT
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-google"></i>
                        Tiếp tục với Google
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-facebook"></i> Tiếp tục với Facebook
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-apple"></i> Tiếp tục với Apple
                    </button>
                </div>

                <hr className="login-divider"/>

                <div className="signup-prompt">
                    <p>Bạn chưa có tài khoản?</p>
                    <button className="btn-signup" onClick={()=>navigate('/signup')}>Đăng ký</button>
                </div>
            </div>
        </div>
    );
}
export default Login;