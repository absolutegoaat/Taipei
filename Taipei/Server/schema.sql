CREATE DATABASE IF NOT EXISTS mitmproxy_logs;

CREATE TABLE flows (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    flow_hash VARCHAR(16) NOT NULL,
    session_id VARCHAR(32) NOT NULL,
    timestamp DATETIME(3) NOT NULL,
    session_duration DECIMAL(10,2),
    client_ip VARCHAR(45),
    client_port INT UNSIGNED,
    server_ip VARCHAR(45),
    server_port INT UNSIGNED,
    server_sni VARCHAR(255),
    method VARCHAR(10) NOT NULL,
    scheme VARCHAR(10),
    host VARCHAR(255) NOT NULL,
    port INT UNSIGNED,
    path TEXT,
    pretty_url TEXT,
    http_version VARCHAR(10),
    status_code INT UNSIGNED,
    status_reason VARCHAR(100),
    req_content_length INT UNSIGNED,
    req_is_binary BOOLEAN DEFAULT FALSE,
    req_encoding VARCHAR(20),
    req_truncated BOOLEAN DEFAULT FALSE,
    resp_content_length INT UNSIGNED,
    resp_is_binary BOOLEAN DEFAULT FALSE,
    resp_encoding VARCHAR(20),
    resp_truncated BOOLEAN DEFAULT FALSE,
    host_category VARCHAR(50),
    is_https BOOLEAN DEFAULT FALSE,
    has_error BOOLEAN DEFAULT FALSE,
    error_message TEXT,
    req_start DECIMAL(16,6),
    req_end DECIMAL(16,6),
    resp_start DECIMAL(16,6),
    resp_end DECIMAL(16,6),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
)

CREATE TABLE flow_headers (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    flow_id BIGINT UNSIGNED NOT NULL,
    header_type ENUM('request', 'response') NOT NULL,
    header_name VARCHAR(255) NOT NULL,
    header_value TEXT,
    is_sensitive BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
)

CREATE TABLE flow_cookies (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    flow_id BIGINT UNSIGNED NOT NULL,
    cookie_type ENUM('request', 'response') NOT NULL,
    cookie_name VARCHAR(255) NOT NULL,
    cookie_value TEXT,
    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
)

CREATE TABLE flow_query_params (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    flow_id BIGINT UNSIGNED NOT NULL,
    param_name VARCHAR(255) NOT NULL,
    param_value TEXT,
    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
)

CREATE TABLE flow_content (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    flow_id BIGINT UNSIGNED NOT NULL,
    content_type ENUM('request', 'response') NOT NULL,
    text_content LONGTEXT,
    binary_content LONGBLOB,
    is_binary BOOLEAN DEFAULT FALSE,
    was_truncated BOOLEAN DEFAULT FALSE,
    original_size INT UNSIGNED,
    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
)

CREATE TABLE proxy_sessions (
    session_id VARCHAR(32) PRIMARY KEY,
    started_at DATETIME NOT NULL,
    ended_at DATETIME,
    listen_host VARCHAR(45),
    listen_port INT UNSIGNED,
    total_flows INT UNSIGNED DEFAULT 0,
    https_flows INT UNSIGNED DEFAULT 0,
    error_count INT UNSIGNED DEFAULT 0,
    log_file_path VARCHAR(500)
)

CREATE TABLE error_log (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    timestamp DATETIME NOT NULL,
    level VARCHAR(10) NOT NULL,
    source VARCHAR(100),
    message TEXT NOT NULL,
    exception TEXT
)

CREATE INDEX idx_timestamp ON flows(timestamp);
CREATE INDEX idx_host ON flows(host);
CREATE INDEX idx_method ON flows(method);
CREATE INDEX idx_status ON flows(status_code);
CREATE INDEX idx_session ON flows(session_id);
CREATE INDEX idx_host_category ON flows(host_category);
CREATE INDEX idx_has_error ON flows(has_error);
CREATE INDEX idx_client_ip ON flows(client_ip);
CREATE INDEX idx_flow_headers ON flow_headers(flow_id);
CREATE INDEX idx_header_name ON flow_headers(header_name);
CREATE INDEX idx_flow_cookies ON flow_cookies(flow_id);
CREATE INDEX idx_flow_query ON flow_query_params(flow_id);
CREATE INDEX idx_flow_content ON flow_content(flow_id);
CREATE INDEX idx_error_timestamp ON error_log(timestamp);
CREATE INDEX idx_error_level ON error_log(level);
CREATE INDEX idx_session_started ON proxy_sessions(started_at);

CREATE VIEW flow_summary AS
SELECT 
    f.id, 
    f.flow_hash, 
    f.timestamp, 
    f.session_duration, 
    f.method, 
    f.host, 
    f.path, 
    f.status_code, 
    f.host_category,
    f.is_https,
    f.has_error, 
    f.error_message, 
    f.req_content_length, 
    f.resp_content_length, 
    TIMESTAMPDIFF(MICROSECOND, FROM_UNIXTIME(f.req_start), FROM_UNIXTIME(f.resp_end)) / 1000 AS total_ms
FROM flows f
ORDER BY f.timestamp DESC;