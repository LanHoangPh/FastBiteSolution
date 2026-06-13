use std::sync::Mutex;
use tauri::{AppHandle, Manager, State};
use serde::{Deserialize, Serialize};
use std::net::TcpListener;
use std::io::{Read, Write};

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct UserInfo {
    pub id: String,
    pub email: String,
    pub first_name: String,
    pub last_name: String,
    pub full_name: Option<String>,
    pub avatar_url: Option<String>,
    pub bio: Option<String>,
    pub is_active: bool,
    pub roles: Vec<String>,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct AuthResponse {
    pub token_type: String,
    pub access_token: String,
    pub refresh_token: String,
    pub access_token_expires_at: String,
    pub refresh_token_expires_at: String,
    pub user: UserInfo,
}

pub struct SessionState {
    pub session: Mutex<Option<AuthResponse>>,
}

fn get_session_file_path(app: &AppHandle) -> Option<std::path::PathBuf> {
    let mut path = app.path().app_data_dir().ok()?;
    // Ensure the directory exists
    let _ = std::fs::create_dir_all(&path);
    path.push("session.json");
    Some(path)
}

#[tauri::command]
fn save_session(
    session: AuthResponse,
    remember_me: bool,
    state: State<'_, SessionState>,
    app: AppHandle,
) -> Result<(), String> {
    // 1. Update in-memory state
    let mut session_guard = state.session.lock().map_err(|e| e.to_string())?;
    *session_guard = Some(session.clone());

    // 2. Handle persistence
    if let Some(path) = get_session_file_path(&app) {
        if remember_me {
            let json_str = serde_json::to_string(&session).map_err(|e| e.to_string())?;
            std::fs::write(path, json_str).map_err(|e| e.to_string())?;
        } else {
            // Delete file if it exists
            if path.exists() {
                let _ = std::fs::remove_file(path);
            }
        }
    }
    Ok(())
}

#[tauri::command]
fn load_session(
    state: State<'_, SessionState>,
    app: AppHandle,
) -> Result<Option<AuthResponse>, String> {
    // 1. Check in-memory state
    let mut session_guard = state.session.lock().map_err(|e| e.to_string())?;
    if session_guard.is_some() {
        return Ok(session_guard.clone());
    }

    // 2. Check persistent file
    if let Some(path) = get_session_file_path(&app) {
        if path.exists() {
            let content = std::fs::read_to_string(path).map_err(|e| e.to_string())?;
            let session: AuthResponse = serde_json::from_str(&content).map_err(|e| e.to_string())?;
            // Cache in memory
            *session_guard = Some(session.clone());
            return Ok(Some(session));
        }
    }
    Ok(None)
}

#[tauri::command]
fn clear_session(
    state: State<'_, SessionState>,
    app: AppHandle,
) -> Result<(), String> {
    // 1. Clear in-memory
    let mut session_guard = state.session.lock().map_err(|e| e.to_string())?;
    *session_guard = None;

    // 2. Clear persistent file
    if let Some(path) = get_session_file_path(&app) {
        if path.exists() {
            let _ = std::fs::remove_file(path);
        }
    }
    Ok(())
}

fn url_decode(s: &str) -> String {
    let mut res = String::new();
    let mut chars = s.chars();
    while let Some(c) = chars.next() {
        if c == '%' {
            let mut hex = String::new();
            if let Some(h1) = chars.next() { hex.push(h1); }
            if let Some(h2) = chars.next() { hex.push(h2); }
            if let Ok(val) = u8::from_str_radix(&hex, 16) {
                res.push(val as char);
            }
        } else if c == '+' {
            res.push(' ');
        } else {
            res.push(c);
        }
    }
    res
}

// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
#[tauri::command]
async fn start_google_login_flow(app: AppHandle, auth_url: String) -> Result<String, String> {
    println!("=== start_google_login_flow ===");
    println!("Auth URL: {}", auth_url);

    // 1. Start TCP listener on localhost
    let listener = TcpListener::bind("127.0.0.1:14250").map_err(|e| e.to_string())?;
    listener.set_nonblocking(true).map_err(|e| e.to_string())?;

    // 2. Create the Google login webview window from Rust
    let _window = tauri::WebviewWindowBuilder::new(
        &app,
        "google-login",
        tauri::WebviewUrl::External(auth_url.parse::<tauri::Url>().map_err(|e| e.to_string())?)
    )
    .title("Sign in with Google")
    .inner_size(500.0, 600.0)
    .resizable(false)
    .focused(true)
    .user_agent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
    .build()
    .map_err(|e| e.to_string())?;

    let start_time = std::time::Instant::now();
    let timeout = std::time::Duration::from_secs(60);

    let mut captured_token = None;
    let mut error_msg = None;

    loop {
        if start_time.elapsed() > timeout {
            return Err("Google login timed out after 60 seconds".to_string());
        }

        match listener.accept() {
            Ok((mut stream, _)) => {
                let mut buffer = [0; 4096];
                stream.set_read_timeout(Some(std::time::Duration::from_secs(5))).map_err(|e| e.to_string())?;
                let bytes_read = stream.read(&mut buffer).map_err(|e| e.to_string())?;
                let request_str = String::from_utf8_lossy(&buffer[..bytes_read]);

                // Request 1: Google redirect callback (serve HTML with JS hash-forwarder)
                if request_str.contains("GET /callback") {
                    let response_html = r#"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset="utf-8">
                            <title>Authenticating...</title>
                            <style>
                                body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; background-color: #f1f5f9; color: #1e293b; }
                                .card { background: white; padding: 2.5rem; border-radius: 1.5rem; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1); text-align: center; max-w-sm; }
                                h1 { color: #4f46e5; margin-bottom: 0.5rem; font-size: 1.5rem; }
                                p { font-size: 0.95rem; line-height: 1.5; color: #64748b; }
                            </style>
                        </head>
                        <body>
                            <div class="card" id="status-card">
                                <h1 id="status-title">Authenticating...</h1>
                                <p id="status-desc">Please wait while we complete the authentication process.</p>
                            </div>
                            <script>
                                const params = new URLSearchParams(window.location.hash.substring(1) || window.location.search);
                                const idToken = params.get('id_token') || params.get('token');
                                
                                if (idToken) {
                                    fetch('/token?id_token=' + encodeURIComponent(idToken))
                                        .then(res => {
                                            document.getElementById('status-title').innerText = 'Authentication Successful!';
                                            document.getElementById('status-desc').innerText = 'You have logged in successfully. You can now close this tab and return to the application.';
                                        })
                                        .catch(err => {
                                            document.getElementById('status-title').innerText = 'Connection Error';
                                            document.getElementById('status-desc').innerText = 'Failed to communicate with the desktop app. Please try again.';
                                        });
                                } else {
                                    document.getElementById('status-title').innerText = 'Authentication Failed';
                                    document.getElementById('status-desc').innerText = 'No authentication token was found in the URL. Please try again.';
                                    fetch('/token?error=no_token');
                                }
                            </script>
                        </body>
                        </html>
                    "#;

                    let http_response = format!(
                        "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {}\r\nConnection: close\r\n\r\n{}",
                        response_html.len(),
                        response_html
                    );

                    let _ = stream.write_all(http_response.as_bytes());
                    let _ = stream.flush();
                }
                // Request 2: AJAX request from our HTML page carrying the token in query params
                else if request_str.contains("GET /token") {
                    let request_line = request_str.lines().next().unwrap_or("");
                    let path_and_query = request_line.split_whitespace().nth(1).unwrap_or("");
                    
                    let mut id_token = String::new();
                    if let Some(params_start) = path_and_query.find("?") {
                        let query_part = &path_and_query[params_start + 1..];
                        for pair in query_part.split('&') {
                            let mut parts = pair.splitn(2, '=');
                            if let (Some(key), Some(val)) = (parts.next(), parts.next()) {
                                if key == "id_token" {
                                    id_token = url_decode(val);
                                    break;
                                }
                            }
                        }
                    }

                    // Respond to the AJAX fetch call with a 200 OK and allow CORS just in case
                    let http_response = "HTTP/1.1 200 OK\r\nAccess-Control-Allow-Origin: *\r\nContent-Length: 2\r\nConnection: close\r\n\r\nOK";
                    let _ = stream.write_all(http_response.as_bytes());
                    let _ = stream.flush();

                    if !id_token.is_empty() {
                        captured_token = Some(id_token);
                        // Automatically close the Google popup window if it exists
                        if let Some(window) = app.get_webview_window("google-login") {
                            let _ = window.close();
                        }
                        break;
                    } else {
                        error_msg = Some("No ID token found in AJAX request".to_string());
                        break;
                    }
                }
            }
            Err(ref e) if e.kind() == std::io::ErrorKind::WouldBlock => {
                std::thread::sleep(std::time::Duration::from_millis(50));
                continue;
            }
            Err(e) => return Err(e.to_string()),
        }
    }

    if let Some(token) = captured_token {
        Ok(token)
    } else {
        Err(error_msg.unwrap_or_else(|| "Google login failed".to_string()))
    }
}

#[tauri::command]
fn greet(name: &str) -> String {
    format!("Hello, {}! You've been greeted from Rust!", name)
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let mut log_path = std::env::current_dir().unwrap_or_default();
    log_path.push("logs");

    tauri::Builder::default()
        .manage(SessionState {
            session: Mutex::new(None),
        })
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_http::init())
        .plugin(
            tauri_plugin_log::Builder::new()
                .targets([
                    tauri_plugin_log::Target::new(tauri_plugin_log::TargetKind::Stdout),
                    tauri_plugin_log::Target::new(tauri_plugin_log::TargetKind::Webview),
                    tauri_plugin_log::Target::new(tauri_plugin_log::TargetKind::Folder {
                        path: log_path,
                        file_name: Some("app.log".to_string()),
                    }),
                ])
                .build(),
        )
        .invoke_handler(tauri::generate_handler![
            greet,
            save_session,
            load_session,
            clear_session,
            start_google_login_flow
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
