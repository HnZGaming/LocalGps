# LocalGps

Passive mod to allow server mods, scripts and Torch plugins to populate local GPSs in client games, 
in such way that it's more flexible than the vanilla system depending on the use case.

## Background

The vanilla GPS system had following issues:

- They identify GPSs by "hash" which is not reliable in some cases because players can easily manipulate it.
- Local GPSs fail to show up in the client if the tied entity is not replicated.
- There's no option to make the "ding" sound for local GPSs.

These "minor" inconvenience added up in many ocassions and I had to make my own purpose-built system.

## Use Cases

This system focuses on managing "local" GPSs, which are created by the server but won't be saved in the server.
For most event-based GPSs I find it easy to use local GPSs in order to start fresh every session.
This mod is most suited for use cases where the server will constantly update GPSs with different texts, colors etc 
while managing their lifespan based on the presence of entities in the world.


## System

GPSs are populated in the client game using data sent by the server. GPSs will be identified and managed via an ID number (not "hash").
Server can send add/update/remove events using the ID. GPSs can either show up at specific positions or follow specific entities.
For entities that are not replicated, the system will show GPSs at their last known position in the server and move them in an interpolation.
GPSs can make the "ding" sound on creation.

## How to Use

- Add the mod in your world.
- Copy files in `HNZ.LocalGps.Interface` to your mod/plugin project. 
- For mods, give them a proper namespace.
- Instantiate `LocalGpsApi` and start sending events.

## Version Control

Do NOT edit `LocalGpsApi.ModVersion`. If `ModVersion` is different in the receiving end, events will be ignored.
For every "destructive" update, I'll upload a new mod on Steam workshop with a new `ModVersion`, rather than updating or deleting the previous mod.
