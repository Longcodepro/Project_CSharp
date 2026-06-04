import React from 'react';
import '../CSS/Signup_style.css';
function Signup()
{
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

                <form className="signup-form">
                    <div className="input-group">
                        <label htmlFor='email'>Email</label>
                        <input type="text" id="email"/>
                    </div>
                    <button type="button" className="btn-submit">Tiếp tục</button>
                </form>

                <hr className="signup-divider"/>

                <div className="otherSignup">
                    <button className="btn-other">
                        <i className="fa-solid fa-mobile-screen"></i>
                        Đăng ký bằng SĐT
                    </button>
                   <button className="btn-other">
                        <i className="fa-brands fa-google"></i>
                        Đăng ký bằng Google
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-facebook"></i> Đăng ký với Facebook
                    </button>
                    <button className="btn-other">
                        <i className="fa-brands fa-apple"></i> Đăng ký với Apple
                    </button>
                </div>

                <hr className="signup-divider"/>

                <div className="login-promtp">
                    <p>Bạn đã có tài khoản?</p>
                    <button className="btn-login">Đăng nhập</button>
                </div>
            </div>
        </div>
    );
}
export default Signup;