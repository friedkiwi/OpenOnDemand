# OpenOnDemand

This is an open-source implementation of a VOD OnDemand service. It takes a DVB TS URL and an XML TV as an input parameter, and outputs an on-demand service not unlike the ones provided by commercial broadcasters.

## A word of warning...

This codebase is unfinished. It's unlikely it'll work yet, as it currently does not have a finished frontend.


## Compilation

First, copy the `OpenOnDemand/app.config.template` file to `OpenOnDemand/app.config` and update settings to the right values.
 
Afterwards, you should be able to just use `xbuild` in the root directory of the solution.

## Setting up a DVB source feed

TBC

## A bit of background

Recently the BBC started to require you to log in into iPlayer to be able to watch video streams. Some people, including me, aren't particularly fond of sacrificing the information required to be able to access the service. Since there is no alternative anymore to registration, the next best thing is to just reimplement the service so I could self-host it.

