import os

import bcrypt
import mysql.connector
from colorama import Fore, Style
from dotenv import load_dotenv
from mysql.connector import Error

load_dotenv()

dbhost = os.getenv("MYSQL_HOST", os.getenv("DB_HOST", "localhost"))
dbuser = os.getenv("DB_USER", "root")
dbpassword = os.getenv("DB_PASS", "")


class DatabaseManager:
    def __init__(
        self, host=dbhost, user=dbuser, password=dbpassword, database="taipei"
    ):
        self.host = host
        self.user = user
        self.password = password
        self.database = database
        self.initialize_database()

    def get_connection(self):
        return mysql.connector.connect(
            host=self.host,
            user=self.user,
            password=self.password,
            database=self.database,
        )

    def initialize_database(self):
        try:
            conn = mysql.connector.connect(
                host=self.host, user=self.user, password=self.password
            )
            cursor = conn.cursor()
            cursor.execute(f"CREATE DATABASE IF NOT EXISTS {self.database}")
            conn.commit()
            conn.close()
            print(f"{Fore.GREEN}[+] Database '{self.database}' ready.{Style.RESET_ALL}")
            self._create_database()
        except Error as e:
            print(f"{Fore.RED}[-] Error initializing database: {e}{Style.RESET_ALL}")
            raise

    def _hash_token(self, password: str) -> bytes:
        return bcrypt.hashpw(password.encode(), bcrypt.gensalt(rounds=12))

    def _verify_token(self, password: str, hashed) -> bool:
        if isinstance(hashed, str):
            hashed = hashed.encode()
        return bcrypt.checkpw(password.encode(), hashed)

    def _create_database(self):
        """Create new database with users table."""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            # prepare for impact
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS tokens (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    token VARCHAR(255) UNIQUE NOT NULL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS flows (
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
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS flow_headers (
                    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    flow_id BIGINT UNSIGNED NOT NULL,
                    header_type ENUM('request', 'response') NOT NULL,
                    header_name VARCHAR(255) NOT NULL,
                    header_value TEXT,
                    is_sensitive BOOLEAN DEFAULT FALSE,
                    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
                )
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS flow_cookies (
                    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    flow_id BIGINT UNSIGNED NOT NULL,
                    cookie_type ENUM('request', 'response') NOT NULL,
                    cookie_name VARCHAR(255) NOT NULL,
                    cookie_value TEXT,
                    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
                )
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS flow_query_params (
                    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    flow_id BIGINT UNSIGNED NOT NULL,
                    param_name VARCHAR(255) NOT NULL,
                    param_value TEXT,
                    FOREIGN KEY (flow_id) REFERENCES flows(id) ON DELETE CASCADE
                )
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS flow_content (
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
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS proxy_sessions (
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
            """)

            cursor.execute("""
                CREATE TABLE IF NOT EXISTS error_log (
                    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    timestamp DATETIME NOT NULL,
                    level VARCHAR(10) NOT NULL,
                    source VARCHAR(100),
                    message TEXT NOT NULL,
                    exception TEXT
                )
            """)

            index_statements = [
                "CREATE INDEX idx_timestamp ON flows(timestamp)",
                "CREATE INDEX idx_host ON flows(host)",
                "CREATE INDEX idx_method ON flows(method)",
                "CREATE INDEX idx_status ON flows(status_code)",
                "CREATE INDEX idx_session ON flows(session_id)",
                "CREATE INDEX idx_host_category ON flows(host_category)",
                "CREATE INDEX idx_has_error ON flows(has_error)",
                "CREATE INDEX idx_client_ip ON flows(client_ip)",
                "CREATE INDEX idx_flow_headers ON flow_headers(flow_id)",
                "CREATE INDEX idx_header_name ON flow_headers(header_name)",
                "CREATE INDEX idx_flow_cookies ON flow_cookies(flow_id)",
                "CREATE INDEX idx_flow_query ON flow_query_params(flow_id)",
                "CREATE INDEX idx_flow_content ON flow_content(flow_id)",
                "CREATE INDEX idx_error_timestamp ON error_log(timestamp)",
                "CREATE INDEX idx_error_level ON error_log(level)",
                "CREATE INDEX idx_session_started ON proxy_sessions(started_at)",
            ]

            for stmt in index_statements:
                try:
                    cursor.execute(stmt)
                except Error as e:
                    if e.errno == 1061:  # ER_DUP_KEYNAME index already exists
                        pass
                    else:
                        raise

            conn.commit()
            print(
                f"{Fore.GREEN}[+] Database schema created successfully.{Style.RESET_ALL}"
            )
            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error creating database: {e}{Style.RESET_ALL}")
            raise
        except Exception as e:
            print(f"{Fore.RED}[-] Unexpected error: {e}{Style.RESET_ALL}")
            raise

    def authenticate_token(self, token: str):
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            cursor.execute(
                """
                SELECT id, token
                FROM tokens
                WHERE token = %s
            """,
                (token,),
            )

            user_data = cursor.fetchone()
            conn.close()

            if user_data:
                return True
            return None

        except Error as e:
            print(f"{Fore.RED}[-] Error authenticating user: {e}{Style.RESET_ALL}")
            return None

    def log_flow(self, flow_data):
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            cursor.execute(
                """
                INSERT INTO flows (
                    flow_hash, session_id, timestamp, session_duration,
                    client_ip, client_port, server_ip, server_port, server_sni,
                    method, scheme, host, port, path, pretty_url, http_version,
                    status_code, status_reason,
                    req_content_length, req_is_binary, req_encoding, req_truncated,
                    resp_content_length, resp_is_binary, resp_encoding, resp_truncated,
                    host_category, is_https, has_error, error_message,
                    req_start, req_end, resp_start, resp_end
                ) VALUES (
                    %(flow_hash)s, %(session_id)s, %(timestamp)s, %(session_duration)s,
                    %(client_ip)s, %(client_port)s, %(server_ip)s, %(server_port)s, %(server_sni)s,
                    %(method)s, %(scheme)s, %(host)s, %(port)s, %(path)s, %(pretty_url)s, %(http_version)s,
                    %(status_code)s, %(status_reason)s,
                    %(req_content_length)s, %(req_is_binary)s, %(req_encoding)s, %(req_truncated)s,
                    %(resp_content_length)s, %(resp_is_binary)s, %(resp_encoding)s, %(resp_truncated)s,
                    %(host_category)s, %(is_https)s, %(has_error)s, %(error_message)s,
                    %(req_start)s, %(req_end)s, %(resp_start)s, %(resp_end)s
                )
            """,
                flow_data,
            )

            flow_id = cursor.lastrowid
            conn.commit()
            conn.close()
            return flow_id

        except Error as e:
            print(f"{Fore.RED}[-] Error logging flow: {e}{Style.RESET_ALL}")
            return None

    def log_headers(self, flow_id, header_type, headers, sensitive_keys=None):
        sensitive_keys = sensitive_keys or {
            "authorization",
            "cookie",
            "set-cookie",
            "x-api-key",
        }

        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            rows = [
                (flow_id, header_type, name, value, name.lower() in sensitive_keys)
                for name, value in headers.items()
            ]

            cursor.executemany(
                """
                INSERT INTO flow_headers (flow_id, header_type, header_name, header_value, is_sensitive)
                VALUES (%s, %s, %s, %s, %s)
            """,
                rows,
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error logging headers: {e}{Style.RESET_ALL}")

    def log_cookies(self, flow_id, cookie_type, cookies):
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            rows = []
            for name, value in cookies.items():
                if isinstance(value, tuple):
                    value = value[0]  # unwrap (value, attrs) if it slips through
                rows.append((flow_id, cookie_type, name, value))

            if not rows:
                conn.close()
                return

            cursor.executemany(
                """
                INSERT INTO flow_cookies (flow_id, cookie_type, cookie_name, cookie_value)
                VALUES (%s, %s, %s, %s)
            """,
                rows,
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error logging cookies: {e}{Style.RESET_ALL}")

    def log_query_params(self, flow_id, params):
        """Insert query params for a flow."""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            rows = [(flow_id, name, value) for name, value in params.items()]

            cursor.executemany(
                """
                INSERT INTO flow_query_params (flow_id, param_name, param_value)
                VALUES (%s, %s, %s)
            """,
                rows,
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error logging query params: {e}{Style.RESET_ALL}")

    def log_content(
        self,
        flow_id,
        content_type,
        text_content=None,
        binary_content=None,
        is_binary=False,
        was_truncated=False,
        original_size=None,
    ):
        """Insert request/response body content for a flow."""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            cursor.execute(
                """
                INSERT INTO flow_content (
                    flow_id, content_type, text_content, binary_content,
                    is_binary, was_truncated, original_size
                ) VALUES (%s, %s, %s, %s, %s, %s, %s)
            """,
                (
                    flow_id,
                    content_type,
                    text_content,
                    binary_content,
                    is_binary,
                    was_truncated,
                    original_size,
                ),
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error logging content: {e}{Style.RESET_ALL}")

    def start_session(
        self, session_id, started_at, listen_host, listen_port, log_file_path=None
    ):
        """Create a new proxy_sessions record."""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            cursor.execute(
                """
                INSERT INTO proxy_sessions (session_id, started_at, listen_host, listen_port, log_file_path)
                VALUES (%s, %s, %s, %s, %s)
            """,
                (session_id, started_at, listen_host, listen_port, log_file_path),
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error starting session: {e}{Style.RESET_ALL}")

    def end_session(self, session_id, ended_at, total_flows, https_flows, error_count):
        """Update a proxy_sessions record when the proxy stops."""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            cursor.execute(
                """
                UPDATE proxy_sessions
                SET ended_at = %s, total_flows = %s, https_flows = %s, error_count = %s
                WHERE session_id = %s
            """,
                (ended_at, total_flows, https_flows, error_count, session_id),
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error ending session: {e}{Style.RESET_ALL}")

    def log_error(self, timestamp, level, source, message, exception=None):
        """Insert an entry into error_log."""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            cursor.execute(
                """
                INSERT INTO error_log (timestamp, level, source, message, exception)
                VALUES (%s, %s, %s, %s, %s)
            """,
                (timestamp, level, source, message, exception),
            )

            conn.commit()
            conn.close()

        except Error as e:
            print(f"{Fore.RED}[-] Error writing to error_log: {e}{Style.RESET_ALL}")
