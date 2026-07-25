from db.database import DatabaseManager
from utils import get_timestamp


class dbfuncs(DatabaseManager):
    def get_logs(self, limit=50, offset=0):
        try:
            limit = int(limit)
            offset = int(offset)

            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute(
                """
                SELECT * FROM flows
                ORDER BY id DESC
                LIMIT %s OFFSET %s
            """,
                (limit, offset),
            )

            rows = cursor.fetchall()
            conn.close()
            return rows
        except Exception as e:
            self.log_error(
                get_timestamp(), "Critical", "Database", "Failed to get logs", str(e)
            )
            print(f"FAILED TO GET LOGS: {e}")
            return None

    def get_log(self, id):
        try:
            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute(
                """
                SELECT * FROM flows WHERE id = %s
            """,
                (id,)
            )

            row = cursor.fetchone()
            conn.close()
            return row
        except Exception as e:
            self.log_error(
                get_timestamp(), "Critical", "Database", "Failed to get log", str(e)
            )
            print(f"FAILED TO GET LOG: {e}")
            return None

    def get_totals(self):
        try:
            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute("""
                SELECT
                    (SELECT COUNT(*) FROM flows) AS totalrequests,
                    (SELECT COUNT(*) FROM flow_cookies) AS totalcookies,
                    (SELECT COUNT(*) FROM flows WHERE status_code BETWEEN 200 AND 299) AS total2xx,
                    (SELECT COUNT(*) FROM flows WHERE status_code BETWEEN 400 AND 499) AS total4xx,
                    (SELECT COUNT(*) FROM flows WHERE status_code BETWEEN 500 AND 599) AS total5xx;
            """)

            totals = cursor.fetchone()
            conn.close()
            return totals
        except Exception as e:
            self.log_error(
                get_timestamp(), "Critical", "Database", "Failed to get totals", str(e)
            )
            print(f"FAILED TO GET TOTALS: {e}")
            return None

    def get_logs_count(self):
        try:
            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute("SELECT COUNT(*) AS total FROM flows")
            result = cursor.fetchone()
            conn.close()
            return result["total"] if result else 0
        except Exception as e:
            self.log_error(
                get_timestamp(),
                "Critical",
                "Database",
                "Failed to get logs count",
                str(e),
            )
            print(f"FAILED TO GET LOGS COUNT: {e}")
            return 0
