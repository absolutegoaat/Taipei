from flask import Blueprint, jsonify, request
from db.functions import dbfuncs
from utils import validate_token

act = Blueprint("actions", __name__)
fun = dbfuncs()


@act.route("/taipei/api/log/<int:id>")
def get_log(id):
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403

@act.route("/taipei/api/totals")
def get_totals():
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    data = fun.get_totals()
    return jsonify(data)

@act.route("/taipei/api/search", methods=["POST"])
def search():
    #data = get params 
    return