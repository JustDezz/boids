As the main goal of this task was to create the fastest simulation possible, I didn't put too much effort into creating good architecture (especialy in the UI department).
There is definitly room for improvement, for example better spatial partitioning data structure, adding predators and obstacle avoidance, but I don't have enough time left for these tasks, so here are my thought on how to achieve these goals:

Predators should be pretty easy: we just need to add bool IsPredator to the BoidData and change BoidsJob a bit. 
Predators should align to prey boids, not the other predators; preys should steer away from nearby predators. Predators should not be affected by PoIs.
Also adding different FlockSettings for predators seems like a good idea, so it would require us either passing it in as another structure, or creating NativeArray<FlockSettings> and using it. 
Second approach also has a benefit of being able to easily create new flocks (just add flock index to BoidData and then get all the needed info from the array).
For killing prey boids and breeding predators we can create another job, pretty much like the with PoIs.

Obstacle avoidance shouldn't be too difficult either: we could use a job with number of RaycastCommand to see if boid is heading to the obstacle. 
If so, steer towards first ray wich didn't hit anything, or just turn around if there is no such ray.

Better spatial partitioning data structure is a bit trickier and would require implementing several structures to see which is best. 
I know about BVH trees and Octtrees, but haven't implemented them.

Now for the numbers:
Device / Number of boids / Total time to run jobs / Main thread waiting for jobs to complete / FPS

Phone (Samsung S21) / 1000 / 1.25 ms / 0.9 ms / 60 (cap)

Phone (Samsung S21) / 5000 / 8.75 ms / 8 ms / 45

Laptop (AMD Ryzen 7 6800H, 16gb RAM) / 1000 / 0.2 ms / 0.15 ms / 165 (cap)

Laptop (AMD Ryzen 7 6800H, 16gb RAM) / 5000 / 4.8 ms / 4.4 ms / 130

Rendering of all those polygons (cylinder + sphere) takes considerable amount of time compared to the algorithm itself, 
so using cheaper shaders (not default urp-lit) and optimized model will improve performance even further.

Also, increasing world bounds would help a lot, because there would be less boids in one place, so more checks would be skipped by spatial hash grid.
