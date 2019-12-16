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
    parser.add_argument("uri", help="IIS GraphQL API endpoint")
    parser.add_argument("-s", "--source-dir", required=True, help="Path to folder which contains GSM files")
    parser.add_argument("-u", "--username", required=True, help="User's login to GraphQL API")
    parser.add_argument("-p", "--password", required=True, help="User's password to GraphQL API")

    args = parser.parse_args()
    source_dir = args.source_dir if isabs(args.source_dir) else join(getcwd(), args.source_dir)

    try:
        token = authenticate(args.uri, args.username, args.password)
    except Exception as error:
        print("Unable to authenticate")
        raise error

    for file_name in list_intercepts(source_dir):
        try:
            media_id = upload_media(token, file_name, source_dir, args.uri)
            response = upload_meta(token, file_name, source_dir, args.uri, media_id, args.username, args.password)
            if 'errors' in response:
                print(json.dumps(response, indent=2))
            else:
                print("Successfully uploaded %s"%(file_name))
        except Exception as err:
            print("Unable to process and upload GSM file: %s. Skipping..."%(file_name))
            continue


def authenticate(uri, username, password):
    payload = {
        "operationName": "login",
        "variables": { "username": username, "password": password },
        "query": """
            mutation login($username: String!, $password: String!) {
                login(username: $username, password: $password) {
                    token
                }
            }
        """
    }
    response = requests.post(uri, json=payload).json()
    return response['data']['login']['token']


def list_intercepts(source):
    try:
        return list(set([splitext(f)[0].split(sep="_", maxsplit=1)[1] for f in listdir(source)]))
    except Exception:
        raise Exception("Data was not found in " + source)


def upload_media(token, file_name, source_dir, uri):
    filename = file_name + ".wav"
    headers = { 'Authorization': token }
    with open(join(source_dir, "Voice_" + filename), mode="rb") as f:
        response = requests.post(uri + "/api/files", headers=headers, files={"file": (filename, f, 'audio/wav')})
        return response.json()["id"]


def upload_meta(token, intercept_name, source, uri, media_id, username, password):
    content = read_meta_file(intercept_name, source)
    data = parse_meta_content(content)
    return send_meta(token, uri, media_id, data, username, password)


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


def send_meta(token, uri, media_id, data, log, passwd):
    query = build_query(media_id, data)
    headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Content-Encoding': 'utf-8',
        'Authorization': token
    }
    print(json.dumps(query, indent=2))
    return requests.post(uri, json=query, headers=headers).json()


def build_query(media_id, data):
    date = datetime.strptime(data["time"], "%d.%m.%Y, %H:%M:%S")
    return {
        "operationName": "uploadGsmMaterial",
        "query": """
            mutation uploadGsmMaterial($input: MaterialInput!) {
                createMaterial(input: $input) {
                    id
                }
            }
        """,
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
                                "value": data.get("phone_number"),
                                "type": "CellphoneSign"
                            }
                        ]
                    }
                },
                "data": [
                    {
                        "type": "Text",
                        "text": json.dumps(data)
                    }
                ]
            }
        }
    }


if __name__ == "__main__":
    main()