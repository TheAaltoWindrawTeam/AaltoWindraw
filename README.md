__AaltoWindraw__
=============

Overview
--------

This project is part of the course _T-111.5350 Multimedia Programming_ at __Aalto University__.

Basically, it is a port of classic boardgame _Pictionary_ to the __Aalto Window platform__.

For people waking up from 30 years of sleep, rules are as follow :
* Two players (or two teams)
* One of them has to draw something
* The other has to guess what this something is

It is kind of similar to some other game:
_Draw Something_ ( _Android_, _Iphone_)

__Aalto Window Platforms__ are _Microsoft PixelSense_ tables buffed up with some awesome tools ( _Kinect_, APIs and so on) and located on the three campus of __Aalto University__.

They prove to be the archetypal platform for the kind of game that __AaltoWindraw__ is.

You can see a video at http://www.youtube.com/watch?v=72FyK5rsrqo

Setup
-----

In order to run AaltoWindraw, you need an instance of MongoDB running.
MongoDB can be downloaded here: http://www.mongodb.org/downloads
It can then be launched aside AaltoWindraw.
If not, AaltoWindraw itself will raise up an instance of MongoDB 
(provided it is reachable via the PATH variable).
An new database is filled de facto with a set of default words so that 
users do not have to manually add them from the beginning.

Some options are to be manually defined according to your needs, they 
are present in the file:
AaltoWindrawModel/Properties/Resources.resx

This way the game can be relatively fine-tuned (name of the station, IP 
of the server, threshold for word recognition and so on).
