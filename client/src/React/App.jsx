import React, { useState } from 'react';
import {Routes, Route, useNavigate} from 'react-router-dom';
import Login from './Login.jsx';
import Signup from './Signup.jsx'
import OTP from './otp.jsx';
import PhoneSignup from './Phone_Signup.jsx';
import '../CSS/App_style.css';


function TopView() {
  const navigate=useNavigate();
  return (
    <div style={{ display: 'flex', alignItems: 'center', padding:'8px 20px', color:'white', height:'100%'}}>
      
      {/* Cụm Logo và Home */}
      <div style={{ display: 'flex', alignItems: 'center' }}>
        <img src="https://storage.googleapis.com/pr-newsroom-wp/1/2018/11/Spotify_Logo_RGB_White.png"
            alt="Spotify Logo"
            style={{ width: '110px', marginRight: '20px', cursor: 'pointer' }}
            onClick={()=>navigate('/')}
        />
        <div className="btn-home" title="Trang chủ" onClick={()=>navigate('/')}>
          <i className="fa-solid fa-house" style={{ color: 'white', fontSize: '18px' }}></i>
        </div>
      </div>

      {/* Cụm Tìm kiếm ở chính giữa */}
      <div style={{ flex: 1, display: 'flex', justifyContent: 'center' }}>
        <div style={{display:'flex', width:'100%', maxWidth:'450px', position:'relative'}}>
          {/* Đã sửa lỗi transLateY thành translateY */}
          <i className="fa-solid fa-magnifying-glass" style={{position:'absolute', left:'15px', top:'50%', transform:'translateY(-50%)', color: '#b3b3b3', fontSize: '18px'}}></i>
          <input type="text" className="btn-found" placeholder="Bạn muốn phát nội dung gì?"></input>
        </div>
      </div>
      
      {/*Cụm chức năng bên phải*/}
      <div style={{display: 'flex', alignItems:'center', gap:'24px'}}>
        <span className="text-link" title="Nâng cấp lên Premium">Premium</span>
        <span className="text-link">Thống kê</span>
      </div>
     
      <div className="btn-notification" title="có gì mới">
        <i className="fa-solid fa-bell" style={{color:'white', fontSize:'18px'}}></i>
      </div>

      <div style={{display:'flex',alignItems:'center', gap:'16px',borderLeft:'1px solid #333',paddingLeft:'24px'}}>
        <button className="text-link" onClick={()=>navigate('/signup')}>Đăng ký</button>
        <button className="btn-login" onClick={()=>navigate('/login')}>Đăng nhập</button>
      </div>
    </div>
  );
}
// 1. Thành phần Sidebar (Thanh bên trái)
function Sidebar() {
  return (
    <div className="sidebar-container">
      <div className="Side-view">
        <div className="Side-window">
          <i className="fa-solid fa-window-maximize"></i>
        </div>
        <span style={{fontWeight:'bold'}}>Thư viện</span>

        <div className="btn-plus" title="Tạo danh sách phát hoặc thư mục">
          <i className="fa-solid fa-plus"></i>
        </div>
        <div className="btn-expand" title="Hiển thị thêm">
          <i className="fa-solid fa-arrow-right"></i> {/* Spotify xài mũi tên ngang cho nút này */}
        </div>
      </div>
      {/* Khu vực để chứa playlist sau này */}
      <div style={{ padding: '0 16px' }}>
         {/* Sau này bạn có thể map() danh sách bài hát / playlist vào đây */}
      </div>
    </div>

  );
}

// 2. Thành phần MainView (Khu vực nội dung chính)
function MainView({onPlaySong}) {
  //Dữ liệu giả lập cho MainView (Playlist database-sau này chèn database vào đây)-chủ đề game
  const genshinPlaylists = [
    {
      id: 1,
      title: "Liyue OST",
      desc: "Bản giao hưởng rực rỡ và bình yên của vùng đất Nham.",
      imgUrl: "https://placehold.co/300x300/eab676/000?text=Liyue",
    },
    {
      id: 2,
      title: "Fontaine Mix",
      desc: "Những giai điệu dưới mặt nước và thẩm phán tối cao.",
      imgUrl: "https://placehold.co/300x300/76c4ea/000?text=Fontaine",
    },
    {
      id: 3,
      title: "Trận Chiến Ác Liệt",
      desc: "Nhạc nền đánh Boss cực mạnh để chạy deadline.",
      imgUrl: "https://placehold.co/300x300/ea7676/000?text=Boss+Fight",
    },
    {
      id: 4,
      title: "Quán Rượu Mondstadt",
      desc: "Nơi bắt đầu của mọi chuyến phiêu lưu.",
      imgUrl: "https://placehold.co/300x300/9eea76/000?text=Mondstadt",
    },
    {
      id: 5,
      title: "Teyvat Lofi",
      desc: "Chill cùng âm nhạc phiêu lưu nốt trầm.",
      imgUrl: "https://placehold.co/300x300/a376ea/000?text=Lofi",
    }
  ];
  const tiktokTrending = [
    {
      id: 101,
      title: "Viral Hits 2026",
      desc: "Những bài hát đang tạo trend mạnh mẽ nhất.",
      imgUrl: "https://placehold.co/300x300/181818/FFF?text=Hits",
    },
    {
      id: 102,
      title: "Phonk Gym",
      desc: "Nhạc tập tạ Push/Pull/Legs đẩy năng lượng lên nóc.",
      imgUrl: "https://placehold.co/300x300/3d3d3d/FFF?text=Phonk",
    },
    {
      id: 103,
      title: "Speed Up Mix",
      desc: "Phiên bản tua nhanh bắt tai của các ca khúc hot.",
      imgUrl: "https://placehold.co/300x300/8c8c8c/000?text=Speed+Up",
    }
  ];
  const renderCard = (item) => (
    <div className="card" key={item.id} onClick={()=>onPlaySong(item)}>
      <div className="card-image-container">
        <img src={item.imgUrl} alt={item.title} className="card-image" />
        <button className="btn-play-card">
          <i className="fa-solid fa-play"></i>
        </button>
      </div>
      <div className="card-title">{item.title}</div>
      <div className="card-desc">{item.desc}</div>
    </div>
  );
  return (
    <div className="main-view-container">
      {/* Vùng Danh sách 1 */}
      <h2 className="section-title">Nhạc Nền Genshin Impact</h2>
      <div className="cards-grid">
        {/* Dùng hàm .map() để duyệt qua mảng dữ liệu và in ra các thẻ Card */}
        {genshinPlaylists.map(renderCard)}
      </div>

      {/* Vùng Danh sách 2 */}
      <h2 className="section-title">Thịnh Hành Trên TikTok</h2>
      <div className="cards-grid">
        {tiktokTrending.map(renderCard)}
      </div>
    </div>
  );
}

// 3. Thành phần Player (Thanh phát nhạc dưới cùng)
function Player({currentSong}) {
  return (
    <div className="player-container">
      <div className="player-left">
        {
          currentSong ? (
          <>
          <img src={currentSong.imgUrl} alt={currentSong.title} className="player-cover" />
          <div className="player-info">
            <span className="player-title">{currentSong.title}</span>
            <span className="player-desc">Spotify User</span>
          </div>
          <i className="fa-regular fa-heart control-icon" style={{ marginLeft: '10px' }}></i>
          </>
          ):
          (<span style={{ fontSize: '13px', color: '#b3b3b3' }}>Chưa chọn bài hát nào</span>)
        }
      </div>
      {/* KHU VỰC Ở GIỮA: NÚT BẤM VÀ THANH CHẠY NHẠC */}
      <div className="player-center">
        <div className="player-controls">
          <i className="fa-solid fa-shuffle control-icon"></i>
          <i className="fa-solid fa-backward-step control-icon"></i>
          <i className="fa-solid fa-circle-play play-btn"></i>
          <i className="fa-solid fa-forward-step control-icon"></i>
          <i className="fa-solid fa-repeat control-icon"></i>
        </div>
        <div className="playback-bar">
          <span>{currentSong ? "1:15" : "-:--"}</span>
          <div className="progress-bar-container">
            <div className="progress-bar-fill"></div>
          </div>
          <span>{currentSong ? "3:45" : "-:--"}</span>
        </div>
      </div>
      {/* KHU VỰC BÊN PHẢI: ÂM LƯỢNG */}
      <div className="player-right">
        <i className="fa-solid fa-microphone control-icon"></i>
        <i className="fa-solid fa-list-ul control-icon"></i>
        <i className="fa-solid fa-desktop control-icon"></i>
        <i className="fa-solid fa-volume-high control-icon"></i>
        <div className="volume-bar-container">
          <div className="progress-bar-fill" style={{width: '50%'}}></div>
        </div>
      </div>
    </div>
  );
}

function Dashboard({ activeSong, setActiveSong })
{
  return(
    <div className="spotify-app">
      <div className="spotify-header">
        <TopView/>
      </div>
      <div className="spotify-body">
        <Sidebar/>
        <MainView onPlaySong={setActiveSong}/>
      </div>
      <div className="spotify-footer">
        <Player currentSong={activeSong}/>
      </div>
    </div>
  );
}
// 4. Thành phần chính kết hợp tất cả lại
function App() {
  const [activeSong, setActiveSong] = useState(null);
  return (
    
    <Routes>
      <Route path="/" element={<Dashboard activeSong={activeSong} setActiveSong={setActiveSong}/>}/>
      <Route path="/login" element={<Login />}/>
      <Route path="/signup" element={<Signup />}/>
      <Route path="/otp" element={<OTP />}/>
      <Route path="/Phone_Signup" element={<PhoneSignup />}/>
    </Routes>
  );
}

export default App;