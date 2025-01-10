To run both the server and the test console right click the Solution -> Properties -> Multiple Startup Project -> set Start on GameServer.Application and GameServer.TestApp -> Apply and then Start

Jsons for Postman:

Player1:

{
  "Path": "/login",
  "MessageContent": {
    "DeviceId": "62d6b0d7-c885-4d2c-8b82-4b3fbc518e3f"
  }
}

{
  "Path": "/resources/update",
  "MessageContent": {
    "DeviceId": "62d6b0d7-c885-4d2c-8b82-4b3fbc518e3f",
    "FriendPlayerId": "8c64bc82-9db8-43ec-b310-09614dfe2395",
    "ResourceType":1,
    "ResourceValue":10
  }
}

{
  "Path": "/resources/send",
  "MessageContent": {
    "DeviceId": "62d6b0d7-c885-4d2c-8b82-4b3fbc518e3f",
    "FriendPlayerId": "8c64bc82-9db8-43ec-b310-09614dfe2395",
    "ResourceType":1,
    "ResourceValue":10
  }
}

Player2:

{
  "Path": "/login",
  "MessageContent": {
    "DeviceId": "803efc45-43fc-4266-823e-085a1350a230"
  }
}