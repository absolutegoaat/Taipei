from db.database import DatabaseManager
from utils import get_timestamp

class SearchDatabase(DatabaseManager):
    def searchby_ip(self, ip):
        try:
            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute("""
                SELECT * FROM flows
                WHERE client_ip LIKE %s
                LIMIT 25
            """, (f"%{ip}%",))
            return cursor.fetchall()
        except Exception as e:
            self.log_error(
                get_timestamp(), "Critical", "Database", "Failed to get searched data", str(e)
            )
            print(f"FAILED TO GET SEARCH: {e}")
            return None

    def searchby_host(self, host):
        try:
            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute("""
                SELECT * FROM flows
                WHERE host LIKE %s
                LIMIT 25
            """, (f"%{host}%",))
            return cursor.fetchall()
        except Exception as e:
            self.log_error(
                get_timestamp(), "Critical", "Database", "Failed to get searched data", str(e)
            )
            print(f"FAILED TO GET SEARCH: {e}")
            return None
        
    def searchby_path(self, path):
        try:
            conn = self.get_connection()
            cursor = conn.cursor(dictionary=True)
            cursor.execute("""
                SELECT * FROM flows
                WHERE path LIKE %s
                LIMIT 25
            """, (f"%{path}%",))
            return cursor.fetchall()
        except Exception as e:
            self.log_error(
                get_timestamp(), "Critical", "Database", "Failed to get searched data", str(e)
            )
            print(f"FAILED TO GET SEARCH: {e}")
            return None