from flask import Blueprint, jsonify
from db.functions import dbfuncs

act = Blueprint("actions", __name__)
fun = dbfuncs()


@act.route("/taipei/api/log/<int:id>")
def get_log(id):
    pass

@act.route("/taipei/api/totals")
def get_totals():
    data = fun.get_totals()
    return jsonify(data)