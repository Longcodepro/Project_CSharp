-- Seed Data for TuneVault

-- Users: 1 Listener, 1 Artist
-- Passwords are all 'Password123!' which hashes to 'AQAAAA...'.
-- Using placeholder GUIDs for UserIds. Replace with actual GUIDs if needed for specific tests.

-- Listener User
INSERT INTO Users (UserId, DisplayName, Email, PasswordHash, CreatedAt, IsActive, Role)
SELECT '00000000-0000-0000-0000-000000000001', 'Listener User', 'listener@example.com', 'AQAAAAEAAC... (replace with actual hash for Password123!)', GETUTCDATE(), 1, 'Listener'
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'listener@example.com');

-- Artist User
INSERT INTO Users (UserId, DisplayName, Email, PasswordHash, CreatedAt, IsActive, Role)
SELECT '00000000-0000-0000-0000-000000000002', 'Artist User', 'artist@example.com', 'AQAAAAEAAC... (replace with actual hash for Password123!)', GETUTCDATE(), 1, 'Artist'
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'artist@example.com');

-- Media Items: 10 items (mix audio/video)
-- Using placeholder GUIDs for MediaIds and ArtistId.
-- Ensure ArtistId '00000000-0000-0000-0000-000000000002' exists.

INSERT INTO MediaItems (MediaId, Title, Description, FilePath, ContentType, DurationSeconds, MediaType, ArtistId, CreatedAt, IsPublic) VALUES
('10000000-0000-0000-0000-000000000001', 'Chill Lo-fi Beat', 'Relaxing lo-fi hip hop track.', '/uploads/audio/chill_lofi.mp3', 'audio/mpeg', 180, 0, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('10000000-0000-0000-0000-000000000002', 'Epic Orchestral Score', 'A powerful orchestral piece for cinematic moments.', '/uploads/audio/epic_score.mp3', 'audio/mpeg', 300, 0, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('10000000-0000-0000-0000-000000000003', 'Acoustic Guitar Melody', 'Simple and soothing acoustic guitar.', '/uploads/audio/acoustic_melody.wav', 'audio/wav', 120, 0, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('10000000-0000-0000-0000-000000000004', 'Synthwave Drive', 'Retro 80s synthwave track for night drives.', '/uploads/audio/synthwave_drive.mp3', 'audio/mpeg', 240, 0, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('10000000-0000-0000-0000-000000000005', 'Nature Sounds: Forest', 'Immersive forest ambiance.', '/uploads/audio/forest_ambiance.webm', 'audio/webm', 600, 0, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('20000000-0000-0000-0000-000000000001', 'Short Animation Clip', 'A brief animated sequence.', '/uploads/video/short_animation.mp4', 'video/mp4', 30, 1, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('20000000-0000-0000-0000-000000000002', 'Tutorial: Basic Editing', 'Introduction to video editing techniques.', '/uploads/video/editing_tutorial.webm', 'video/webm', 900, 1, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('20000000-0000-0000-0000-000000000003', 'Travel Vlog Snippet', 'A quick look at a beautiful travel destination.', '/uploads/video/travel_vlog.mp4', 'video/mp4', 180, 1, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('20000000-0000-0000-0000-000000000004', 'Music Video: Live Performance', 'A recording of a live music performance.', '/uploads/video/live_performance.mp4', 'video/mp4', 480, 1, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1),
('20000000-0000-0000-0000-000000000005', 'Abstract Visualizer', 'An abstract visualizer synced to music.', '/uploads/video/abstract_visualizer.webm', 'video/webm', 360, 1, '00000000-0000-0000-0000-000000000002', GETUTCDATE(), 1);

-- Playlists: 2 playlists for the Listener User
-- Using placeholder GUIDs for PlaylistIds and UserId.
-- Ensure UserId '00000000-0000-0000-0000-000000000001' exists.

INSERT INTO Playlists (PlaylistId, Name, Description, UserId, CreatedAt, IsPublic) VALUES
('30000000-0000-0000-0000-000000000001', 'My Favorite Chill Tracks', 'A collection of relaxing tunes.', '00000000-0000-0000-0000-000000000001', GETUTCDATE(), 0),
('30000000-0000-0000-0000-000000000002', 'Workout Motivation Mix', 'High-energy tracks for workouts.', '00000000-0000-0000-0000-000000000001', GETUTCDATE(), 0);

-- Playlist Tracks: Add some tracks to the playlists
-- Using placeholder GUIDs for PlaylistTrackId, PlaylistId, and MediaId.
-- Ensure PlaylistIds and MediaIds exist from previous inserts.

-- Tracks for 'My Favorite Chill Tracks' Playlist
INSERT INTO PlaylistTracks (PlaylistTrackId, PlaylistId, MediaId, AddedAt) VALUES
('40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', GETUTCDATE()),
('40000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000003', GETUTCDATE()),
('40000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000004', GETUTCDATE());

-- Tracks for 'Workout Motivation Mix' Playlist
INSERT INTO PlaylistTracks (PlaylistTrackId, PlaylistId, MediaId, AddedAt) VALUES
('40000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002', GETUTCDATE()),
('40000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000004', GETUTCDATE()),
('40000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000001', GETUTCDATE());

-- IMPORTANT:
-- 1. Replace placeholder GUIDs with actual GUIDs if you have specific requirements.
-- 2. Replace 'AQAAAAEAAC... (replace with actual hash for Password123!)' with the actual BCrypt hash for the password 'Password123!'.
--    You can generate this hash using a tool or a separate C# script.
-- 3. Ensure the file paths in MediaItems exist on the server or are correctly mapped by your file storage service.
-- 4. This script assumes the existence of tables: Users, MediaItems, Playlists, PlaylistTracks, Follow.
-- 5. Run this script using a SQL client connected to your database.