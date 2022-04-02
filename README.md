# Flash GPS

API to help you make a custom GPS system in multiplayer.

* Create and manage GPS entities based on your own custom ID model, not "hash".
* Save GPS entities in your own custom persistence model, not the world save.

This API will open up interesting use cases of GPS as a world-space HUD element.

## Background

Vanilla GPS API comes with its own ID/persistency framework which is painful to work with in multiplayer:

- GPS is identified by "hash" which is easily disintegrated by player action.
- "Local GPS" is not thoroughly implemented around entity replication over network.
- The way GPS entities are carried around in the server-side code is just messed up.

I've had enough of it and started making my own API.
GPS is just a world-space HUD element and should be the easiest thing to control.

## System

API is push-based and the server is responsible for managing the display condition of all GPS entities.
Client will simply add/update/remove GPS entities on the HUD based on their ID and timer.
This model makes it easy to control the procedural aspect of GPS text/position from server scripts.

## How to Use

API is available for mods and Torch plugins.

- Add the mod in your world (for Torch, patch the session loader to force-add the mod).
- Copy files to your project from `HNZ.FlashGps.Interface` directory (for mods, change the namespace accordingly). 
- Instantiate `FlashGpsApi` and start sending GPS entities to clients.

## Version Control

Do NOT edit `FlashGpsApi.ModVersion`. For every version (aka destructive changes), new mod will be uploaded on Steam workshop with new `ModVersion`.
