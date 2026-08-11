import urllib.request
import json

try:
    req = urllib.request.Request("https://api.ethereal.email/createAccount", method="POST")
    req.add_header("Content-Type", "application/json")
    with urllib.request.urlopen(req) as response:
        res = response.read()
        print(res.decode('utf-8'))
except Exception as e:
    print(e)
