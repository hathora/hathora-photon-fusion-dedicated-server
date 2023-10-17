# RunBuildResponse


## Fields

| Field                                                                                                            | Type                                                                                                             | Required                                                                                                         | Description                                                                                                      |
| ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `ContentType`                                                                                                    | *string*                                                                                                         | :heavy_check_mark:                                                                                               | HTTP response content type for this operation                                                                    |
| `RunBuild200TextPlainByteString`                                                                                 | *string*                                                                                                         | :heavy_minus_sign:                                                                                               | Ok                                                                                                               |
| `RunBuild404ApplicationJSONString`                                                                               | *string*                                                                                                         | :heavy_minus_sign:                                                                                               | N/A                                                                                                              |
| `StatusCode`                                                                                                     | *int*                                                                                                            | :heavy_check_mark:                                                                                               | HTTP response status code for this operation                                                                     |
| `RawResponse`                                                                                                    | [UnityWebRequest](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Networking.UnityWebRequest.html) | :heavy_minus_sign:                                                                                               | Raw HTTP response; suitable for custom response parsing                                                          |
| `RunBuild500ApplicationJSONString`                                                                               | *string*                                                                                                         | :heavy_minus_sign:                                                                                               | N/A                                                                                                              |