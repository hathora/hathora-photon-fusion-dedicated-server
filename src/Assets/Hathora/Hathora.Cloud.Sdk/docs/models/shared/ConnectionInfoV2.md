# ConnectionInfoV2

Connection information for the default and additional ports.


## Fields

| Field                                                                                                            | Type                                                                                                             | Required                                                                                                         | Description                                                                                                      | Example                                                                                                          |
| ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `AdditionalExposedPorts`                                                                                         | List<[ExposedPort](../../models/shared/ExposedPort.md)>                                                          | :heavy_check_mark:                                                                                               | N/A                                                                                                              |                                                                                                                  |
| `ExposedPort`                                                                                                    | [ExposedPort](../../models/shared/ExposedPort.md)                                                                | :heavy_minus_sign:                                                                                               | Connection details for an active process.                                                                        |                                                                                                                  |
| `RoomId`                                                                                                         | *string*                                                                                                         | :heavy_check_mark:                                                                                               | Unique identifier to a game session or match. Use the default system generated ID or overwrite it with your own. | 2swovpy1fnunu                                                                                                    |
| `Status`                                                                                                         | [ConnectionInfoV2Status](../../models/shared/ConnectionInfoV2Status.md)                                          | :heavy_check_mark:                                                                                               | `exposedPort` will only be available when the `status` of a room is "active".                                    | active                                                                                                           |