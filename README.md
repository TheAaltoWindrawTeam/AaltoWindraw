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

Use cases
---------

Eventually the game should support two modes, synchronous and asynchronous game.

### Asynchronous mode

1. Player A launches the software.
2. Player A selects the asynchronous mode.
3. Player A selects the Draw mode.
4. Player A draws a card among a hand. On this card is written what he has to draw on the whiteboard.
5. Player A draws on the whiteboard. He can use a limited set of colors, and different brush sizes.
6. Player A writes his name, and sends his drawing to the server.
7. Player A quits the game happily.

#### Some days/hours/minutes later:

8. Player B launches the software.
9. Player B selects the asynchronous mode.
10. Player B selects the Guess mode.
11. Player B can select between various modes: random, by author, etc... Player B selects random.
12. A random drawing made before by another user (maybe Player A!) is progressively displayed on the screen, as if drawn under Player B’s eyes.
13. Player B can answer with the virtual keyboard or handwriting recognition. She guessed right.
14. The time she took to guess is associated with the drawing and her name, in the high scores.
15. Player B quits the game even more happily.

### Synchronous mode

1. Player A launches the software in Otaniemi and selects the synchronous mode.
2. Player B launches the software in Arabia and selects the synchronous mode.
3. Player A and Player B connect to each other.
4. Player A draw a card among a hand.
5. Player A starts drawing. The drawing is displayed in real-time on Player B’s screen.
6. Player A and Player B can communicate with messages if they want during the game. Every answer Player B gives is committed to Player A by the chat.
7. When Player B has guessed, it is her turn to draw.
8. The game continues until one of the players quits.

Milestones
----------

Milestones for a first usable version of the game should be as follow:

### 14 Nov 2012

Mid-term demo (only whiteboard and UI capacities).

### 12 Dec 2012
Workshop. Functional game
(whiteboard capacities, database) with at least asynchronous ability.

### 21 Dec 2012
Final submission. Bug correcting and minor enhancements.
