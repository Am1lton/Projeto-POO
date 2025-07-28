# A Simple O.O.P. project
> A simple project to further improve my understanding of Object
> Oriented Programing while practicing what I've learned.


# Special Notes
> here I wrote some notes about how it was coding specific mechanics of the game


 - ## Player Movement
   Surprisingly for me, coding the player movement was by far the most challenging task. Working with unity physics and getting collision to work how I want was so time consuming that I've decided not to use unity physics on the dash and bouncing ball scripts, which has led to even more headache in the future.
   
   Maybe next time I make a movement script I'll think about using something I learned called "Move and Collide", from what I've seen so far I'd  use physics functions to project my character in the direction of the movement and correct it's movement in case of collision, effectively "sliding" through surfaces. It seems very complicated but I like having so much control over how my character move and interact with the environment.

- ## Enemy
	The enemies were simple enough, I reused some things/ideas from the player movement but adapted to the enemies ledge and wall checks.
	
- ## Dash & BouncingProjectile
	I had first coded bouncing projectile using only transform and physics casts, but due to some problems with layers I've decide to use RigidBody sweep so I can use not only the collider of my object instead of casting with a shape that could possibly not rightfully represent my object's shape, and because it'll use the layer "collision rules" that my object had which would prevent collision with the wrong layer.

	With the dash I started using the RigidBody regular movement with MovePosition in FixedUpdate, but then changed to transform.position with physics casts in a coroutine, that already brought better results but it only really worked when I started using RigidBody SweepTest like the bouncing projectile, I started really liking that function.
	
	Overhaul coding the dash and the bouncing projectile was a pain in the ass, with a bunch of collision checks and vector manipulation, definitely the second hardest task after Movement.

	Beside these two powers the rest of the powers were pretty simple.
	
- ## Double Jump & Wall Jump
	These were easy... usually when I think something is going to be easy it ends up being pretty hard but after all the trouble with the dash and bouncing projectile those two were incredibly easy, besides, not much to say here.
	
	# Final Thoughts
	This project is far from finished, I still have much to do, such as: Multiplayer, more powers, and trying out that "move and collide". Also I tried commenting more on my code and although I did comment more in the beginning the more I wrote the less I commented, It is still something I need to improve a lot.  



