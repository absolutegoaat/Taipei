import os
import sys
import threading
import time
import gzip
import base64
import hashlib
import asyncio
from datetime import datetime
from typing import Optional
from pathlib import Path

from mitmproxy import http, certs
from mitmproxy.options import Options
from mitmproxy.master import Master
from mitmproxy.addons import default_addons

from db.database import DatabaseManager
from utils import get_timestamp

db = DatabaseManager()

LISTEN_HOST = "0.0.0.0"
LISTEN_PORT = 8000
LOG_DIR = "logs"
CA_DIR = os.path.join(os.path.expanduser("~"), ".mitmproxy")
MAX_BODY_SIZE = 10 * 1024 * 1024
SENSITIVE_HEADERS = {
    "authorization",
    "cookie",
    "set-cookie",
    "x-api-key",
    "api-key",
    "session",
}


def ensure_ca():
    """Generate the mitmproxy CA if it does not exist."""
    os.makedirs(CA_DIR, exist_ok=True)
    ca_cert_path = os.path.join(CA_DIR, "taipei-ca.pem")

    if not os.path.exists(ca_cert_path):
        print("[+] Generating new root CA...")
        try:
            store = certs.CertStore.create_store(
                path=Path(CA_DIR),  # ← convert to Path here
                basename="Taipei CA Root",
                key_size=2048,
            )
            print(f"[+] Root CA created at: {ca_cert_path}")
            print(
                f"[+] Also generated: {CA_DIR}/mitmproxy-ca-cert.p12 (for mobile import)"
            )
        except Exception as e:
            db.log_error(
                get_timestamp(),
                "Critical",
                "Certificate Creation",
                "CA generation failed",
                str(e),
            )
            print(f"[!] CA generation failed: {e}")
            sys.exit(1)
    else:
        print("[+] Root CA already present.")


def safe_truncate(text: Optional[str], max_len: int = 5000) -> str:
    """Truncate text safely, mark if truncated."""
    if not text:
        return "[empty]"
    if len(text) > max_len:
        return text[:max_len] + f"\n\n[... truncated {len(text) - max_len} chars ...]"
    return text


def mask_sensitive(value):
    """Mask sensitive header values."""
    if len(value) <= 8:
        return "*" * len(value)
    return value[:4] + "*" * (len(value) - 8) + value[-4:]


def get_content_text(flow_part):
    result = {
        "text": None,
        "encoding": None,
        "truncated": False,
        "size": 0,
        "is_binary": False,
    }

    if not flow_part or not flow_part.content:
        result["text"] = "[empty]"
        return result

    content = flow_part.content
    result["size"] = len(content)

    if flow_part.headers.get("content-encoding") == "gzip":
        try:
            content = gzip.decompress(content)
            result["encoding"] = "gzip"
        except Exception:
            pass

    # Try to decode as text
    try:
        text = content.decode("utf-8", errors="replace")
        if len(text) > MAX_BODY_SIZE:
            text = text[:MAX_BODY_SIZE]
            result["truncated"] = True
        result["text"] = text
    except Exception:
        # Binary content
        result["is_binary"] = True
        result["text"] = base64.b64encode(content[:MAX_BODY_SIZE]).decode("ascii")
        if len(content) > MAX_BODY_SIZE:
            result["truncated"] = True

    return result


def get_host_category(host: str) -> str:
    domain = host.lower()
    if any(x in domain for x in ["google", "gstatic", "googleapis"]):
        return "google"
    elif any(x in domain for x in ["facebook", "fbcdn", "instagram", "whatsapp"]):
        return "meta"
    elif any(x in domain for x in ["microsoft", "live", "office", "outlook"]):
        return "microsoft"
    elif any(x in domain for x in ["apple", "icloud", "mzstatic"]):
        return "apple"
    elif any(x in domain for x in ["amazon", "aws"]):
        return "amazon"
    elif any(x in domain for x in ["bank", "paypal", "stripe", "payment"]):
        return "financial"
    elif any(x in domain for x in ["api", "rest", "graphql"]):
        return "api"
    else:
        return "other"


# TODO: ADD DATABASE LOGGING
class Logger:
    def __init__(self):
        self.lock = asyncio.Lock()
        self.stats = {
            "total": 0,
            "https": 0,
            "errors": 0,
            "by_method": {},
            "by_status": {},
            "start_time": time.time(),
        }
        self.session_id = hashlib.sha256(
            f"{self.stats['start_time']}".encode()
        ).hexdigest()[:32]
        db.start_session(
            session_id=self.session_id,
            started_at=datetime.utcnow(),
            listen_host=LISTEN_HOST,
            listen_port=LISTEN_PORT,
            log_file_path=None,
        )
        print(f"[+] Session ID: {self.session_id}")

    async def log_flow(self, flow: http.HTTPFlow):
        self.stats["total"] += 1

        is_https = flow.request.scheme == "https"
        if is_https:
            self.stats["https"] += 1

        method = flow.request.method
        self.stats["by_method"][method] = self.stats["by_method"].get(method, 0) + 1

        status = flow.response.status_code if flow.response else 0
        status_cat = f"{status // 100}xx" if status else "none"
        self.stats["by_status"][status_cat] = (
            self.stats["by_status"].get(status_cat, 0) + 1
        )

        req_content = get_content_text(flow.request)
        resp_content = get_content_text(flow.response) if flow.response else None

        flow_hash = hashlib.sha256(
            f"{flow.client_conn.address[0]}:{flow.request.timestamp_start}:{flow.request.pretty_url}".encode()
        ).hexdigest()[:16]

        has_error = bool(flow.error)
        if has_error:
            self.stats["errors"] += 1

        flow_data = {
            "flow_hash": flow_hash,
            "session_id": self.session_id,
            "timestamp": datetime.utcnow(),
            "session_duration": round(time.time() - self.stats["start_time"], 2),
            "client_ip": flow.client_conn.address[0],
            "client_port": flow.client_conn.address[1],
            "server_ip": flow.server_conn.address[0]
            if flow.server_conn.address
            else None,
            "server_port": flow.server_conn.address[1]
            if flow.server_conn.address
            else None,
            "server_sni": flow.server_conn.sni if flow.server_conn else None,
            "method": method,
            "scheme": flow.request.scheme,
            "host": flow.request.host,
            "port": flow.request.port,
            "path": flow.request.path,
            "pretty_url": flow.request.pretty_url,
            "http_version": flow.request.http_version,
            "status_code": flow.response.status_code if flow.response else None,
            "status_reason": flow.response.reason if flow.response else None,
            "req_content_length": len(flow.request.content)
            if flow.request.content
            else 0,
            "req_is_binary": req_content.get("is_binary", False),
            "req_encoding": req_content.get("encoding"),
            "req_truncated": req_content.get("truncated", False),
            "resp_content_length": len(flow.response.content)
            if flow.response and flow.response.content
            else 0,
            "resp_is_binary": resp_content.get("is_binary", False)
            if resp_content
            else None,
            "resp_encoding": resp_content.get("encoding") if resp_content else None,
            "resp_truncated": resp_content.get("truncated", False)
            if resp_content
            else None,
            "host_category": get_host_category(flow.request.host),
            "is_https": is_https,
            "has_error": has_error,
            "error_message": str(flow.error) if flow.error else None,
            "req_start": flow.request.timestamp_start,
            "req_end": flow.request.timestamp_end,
            "resp_start": flow.response.timestamp_start if flow.response else None,
            "resp_end": flow.response.timestamp_end if flow.response else None,
        }

        flow_id = db.log_flow(flow_data)

        if flow_id:
            db.log_headers(
                flow_id, "request", dict(flow.request.headers), SENSITIVE_HEADERS
            )
            if flow.response:
                db.log_headers(
                    flow_id, "response", dict(flow.response.headers), SENSITIVE_HEADERS
                )

            db.log_cookies(flow_id, "request", dict(flow.request.cookies))

            if flow.response:
                response_cookies = {
                    name: value_attrs[0]
                    for name, value_attrs in flow.response.cookies.items()
                }
                db.log_cookies(flow_id, "response", response_cookies)

            db.log_query_params(flow_id, dict(flow.request.query))

            db.log_content(
                flow_id=flow_id,
                content_type="request",
                text_content=req_content.get("text")
                if not req_content.get("is_binary")
                else None,
                binary_content=req_content.get("text")
                if req_content.get("is_binary")
                else None,
                is_binary=req_content.get("is_binary", False),
                was_truncated=req_content.get("truncated", False),
                original_size=req_content.get("size", 0),
            )

            if resp_content:
                db.log_content(
                    flow_id=flow_id,
                    content_type="response",
                    text_content=resp_content.get("text")
                    if not resp_content.get("is_binary")
                    else None,
                    binary_content=resp_content.get("text")
                    if resp_content.get("is_binary")
                    else None,
                    is_binary=resp_content.get("is_binary", False),
                    was_truncated=resp_content.get("truncated", False),
                    original_size=resp_content.get("size", 0),
                )

        status_str = f"{status}" if flow.response else "NO-RESP"
        content_hint = ""
        if resp_content and not resp_content.get("is_binary"):
            preview = resp_content.get("text", "")[:60].replace("\n", " ")
            content_hint = f" | Body: {preview}..."
        print(
            f"[{self.stats['total']:04d}] {method:6} {status_str:5} {flow.request.pretty_url[:80]}{content_hint[:40]}"
        )

    def request(self, flow: http.HTTPFlow):
        pass

    def response(self, flow: http.HTTPFlow):
        asyncio.ensure_future(self.log_flow(flow))

    def done(self):
        duration = round(time.time() - self.stats["start_time"], 2)
        db.end_session(
            session_id=self.session_id,
            ended_at=datetime.utcnow(),
            total_flows=self.stats["total"],
            https_flows=self.stats["https"],
            error_count=self.stats["errors"],
        )
        print(
            f"\n[!] Session ended. Stats: {self.stats['total']} flows, {self.stats['https']} HTTPS, {self.stats['errors']} errors"
        )
        print(f"[!] Duration: {duration}s")


class ContentInjector:
    def response(self, flow: http.HTTPFlow):
        pass
        # Example: Inject a script into HTML pages
        # if flow.response and "text/html" in flow.response.headers.get("content-type", ""):
        #     html = flow.response.get_text(strict=False)
        #     if html:
        #         payload = "<script>alert('I.M.P. was here, bitch!')</script>"  geniunely shut up kimi
        #         flow.response.text = html.replace("</body>", payload + "</body>")


# --- Master setup ---
class ProxyMaster(Master):
    async def run(self):
        self.addons.add(*default_addons())
        self.logger_addon = Logger()
        self.addons.add(self.logger_addon)
        # Optionally add content injector
        # self.addons.add(ContentInjector())

        await super().run()


async def _start_prox_async():
    print("taipei proxy")
    ensure_ca()

    opts = Options(
        listen_host=LISTEN_HOST,
        listen_port=LISTEN_PORT,
    )
    opts.add_option("ssl_insecure", bool, True, "Ignore upstream cert errors")
    opts.add_option("confdir", str, CA_DIR, "Configuration directory")

    master = ProxyMaster(opts)

    print(f"[+] Proxy listening on {LISTEN_HOST}:{LISTEN_PORT}")
    print(f"[+] Install root CA from {CA_DIR}/mitmproxy-ca-cert.pem on client devices.")
    print(f"[+] For mobile: use {CA_DIR}/mitmproxy-ca-cert.p12")
    print("[+] Press Ctrl+C to stop...")
    print("-" * 60)

    try:
        await master.run()
    except Exception as e:
        db.log_error(
            get_timestamp(), "Critical", "Taipei Proxy Entry", "Fatal error", str(e)
        )
        print(f"[!] Fatal error: {e}")
        raise
    finally:
        master.shutdown()


def _run_proxy_thread():
    """Runs in a separate thread with its own event loop, forever, until the app exits."""
    try:
        asyncio.run(_start_prox_async())
    except Exception as e:
        db.log_error(
            get_timestamp(), "Critical", "Taipei Proxy Entry", "Fatal error", str(e)
        )
        print(f"[!] Fatal error: {e}")


def start_prox():
    ensure_ca()
    thread = threading.Thread(target=_run_proxy_thread, daemon=True)
    thread.start()
    return thread
