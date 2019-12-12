#! /usr/bin/env python
# -*- coding: utf-8 -*-

import json
from datetime import datetime
from os import listdir, getcwd
from os.path import *
from argparse import ArgumentParser
import requests

KEYS_MAP = {
    "Дата, время":      "time",
    "Тип":              "type",
    "Длительность":     "duration",
    "Направление":      "direction",
    "Раcстояние до БС": "distance_to_bs",
    "Вид звонка":       "call_type",
    "Номер телефона":   "phone_number",
    "IMEI":             "imei",
    "ІMSI":             "imsi",
    "TMSI":             "tmsi",
    "Провайдер":        "provider",
    "LAC":              "lac",
    "Массив":           "array",
    "Объект":           "object",
    "Тематика":         "thread",
    "Подразделение":    "subdivision",
    "Регион":           "region",
    "Комментарий":      "comment",
}


def main():
    parser = ArgumentParser()
    parser.add_argument("-u", "--uri", required=True)
    parser.add_argument("-s", "--source", default="")
    parser.add_argument("-l", "--log", default="", required=True)
    parser.add_argument("-p", "--passwd",default="", required=True)

    args = parser.parse_args()
    source = args.source if isabs(args.source) else join(getcwd(), args.source)

    for intercept_name in list_intercepts(source):
        try:
            media_id = upload_media(intercept_name, source, args.uri)
            response = upload_meta(intercept_name, source, args.uri, media_id, args.log, args.passwd)
            print(response)
        except Exception:
            print("FAIL. ", intercept_name, " всрався. Moving next item...")
            continue


def get_auth_token(uri, log, passwd):
    curl = {"operationName": "login","variables":{"username": log,"password": passwd},"query":"mutation login($username: String!, $password: String!) \
    { login(username: $username, password: $password) { token } }"}
    res = requests.post(uri, json=curl)
    auth_token = res.json()['data']['login']['token']
    return auth_token


def list_intercepts(source):
    try:
        return list(set([splitext(f)[0].split(sep="_", maxsplit=1)[1] for f in listdir(source)]))
    except Exception:
        raise Exception("Data was not found in " + source)


def upload_media(intercept_name, source, uri):
    filename = intercept_name + ".wav"
    with open(join(source, "Voice_" + filename), mode="rb") as f:
        response = requests.post(uri + "/api/files", files={"file": (filename, f, 'audio/x-wav')})
        return json.loads(response.text)["id"]


def upload_meta(intercept_name, source, uri, media_id, log, passwd):
    content = read_meta_file(intercept_name, source)
    data = parse_meta_content(content)
    return send_meta(uri, media_id, data, log, passwd)


def read_meta_file(intercept_name, source):
    with open(join(source, "Info_" + intercept_name + ".txt"), mode="r", encoding="windows-1251") as f:
        return f.read()


def parse_meta_content(content):
    tokens = [part.strip() for line in content.splitlines() for part in line.split(sep=":", maxsplit=1)]
    current_key = KEYS_MAP[tokens[0]]
    data = {current_key: ""}
    for token in tokens[1:]:
        if token in KEYS_MAP.keys():
            data[KEYS_MAP[token]] = ""
            current_key = KEYS_MAP[token]
        else:
            data[current_key] += token
    return data


def send_meta(uri, media_id, data, log, passwd):
    query = build_query(media_id, data)
    headers = {
        'Content-type': 'application/json',
        'Accept': 'application/json',
        'Content-Encoding': 'utf-8',
        'Authorization': get_auth_token(uri, log, passwd)
    }
    return requests.post(uri, data=json.dumps(query), headers=headers)


def build_query(media_id, data):
    date = datetime.strptime(data["time"], "%d.%m.%Y, %H:%M:%S")
    phone_number = data.get("phone_number")
    json_string = json.dumps(data)
    return {
        "query": "mutation($input:MaterialInput!){createMaterial(input:$input){id}}",
        "variables": {
            "input": {
                "fileId": media_id,
                "metadata": {
                    "type": "cell.voice",
                    "source": "GsmFiles",
                    "date": date.strftime("%Y-%m-%dT%H:%M:%S"),
                    "features": {
                        "nodes": [
                            {
                                "relation": "Source",
                                "value": phone_number,
                                "type": "CellphoneSign"
                            }
                        ]
                    }
                },
                "data": [
                    {
                        "type": "Text",
                        "text": json_string
                    }
                ]
            }
        }
    }


if __name__ == "__main__":
    main()