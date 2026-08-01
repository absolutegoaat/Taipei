from flask import Blueprint, jsonify, request
from db.functions import dbfuncs
from db.search import SearchDatabase
from utils import validate_token

act = Blueprint("actions", __name__)
fun = dbfuncs()
sea = SearchDatabase()


@act.route("/taipei/api/log/<int:id>")
def get_log(id):
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    return jsonify(fun.get_log(id))


@act.route("/taipei/api/totals")
def get_totals():
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    data = fun.get_totals()
    return jsonify(data)


@act.route('/taipei/api/search', methods=['GET'])
def search():
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    query = request.args.get('query')
    filter_field = request.args.get('filter')

    if not query and filter_field:
        return jsonify({"error": "Missing query parameter"}), 400

    match filter_field:
        case "ip":
            return jsonify(sea.searchby_ip(query))
        case "host":
            return jsonify(sea.searchby_host(query))
        case "path":
            return jsonify(sea.searchby_path(query))
        case _:
            return jsonify({"error": "filter field is invalid"})

@act.route("/taipei/api/log/<int:id>/cookies")
def get_cookies(id):
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    return jsonify(fun.get_cookies(id))


@act.route("/taipei/api/log/<int:id>/headers")
def get_headers(id):
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    return jsonify(fun.get_headers(id))


@act.route("/taipei/api/log/<int:id>/content")
def get_content(id):
    if not validate_token(request.headers.get("taipei-auth")):
        return "Unauthorized", 403
    return jsonify(fun.get_content(id))
