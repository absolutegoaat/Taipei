from flask import Flask, request, jsonify

from db.functions import dbfuncs
from db.database import DatabaseManager

from proxy import start_prox
from actions import act

from utils import validate_token

app = Flask(__name__)

dbfun = dbfuncs()
db = DatabaseManager()

app.register_blueprint(act)

@app.route("/")
def index():
    return "<h1>taipei proxy server api by absolutegoaat on github</h1>"


@app.route("/taipei/get_logs", methods=["GET"])
def get_logs():
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403

    limit = request.args.get("limit", default=50, type=int)
    offset = request.args.get("offset", default=0, type=int)

    logs = dbfun.get_logs(limit=limit, offset=offset)
    total = dbfun.get_logs_count()

    return jsonify({
        "results": logs,
        "limit": limit,
        "offset": offset,
        "total": total
    })


@app.route("/validate", methods=["GET"])
def validate():
    token = request.headers.get("taipei-auth")
    if token is None:
        return "Token is missing", 400
    if db.authenticate_token(token):
        return "Token is valid", 200
    return "Token is invalid", 403



if __name__ == "__main__":
    start_prox()
    app.run(host="0.0.0.0", port=5000, debug=True)
