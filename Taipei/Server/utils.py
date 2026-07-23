import datetime
from db.database import DatabaseManager

db = DatabaseManager()

def get_timestamp():
    return datetime.datetime.now()
    
def validate_token(token):
    return db.authenticate_token(token)