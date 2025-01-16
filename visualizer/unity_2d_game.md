# Introduction

This is to learn more about how unity works, and what we can do in unity in general. This would be based on the following tutorial [here](https://www.youtube.com/watch?v=XtQMytORBmM). I think this is highly recommended in general.

Ok so lets get started

## Installation

I'm using a mac, so I had to install rosetta to interface with unity hub. Once that is done, you can get started with using unity, as you please, but in my case, I will try to understand more about how things work and what they do in order to better understand the platform as well.

Essentially under `Installs` what you want to do is install things that you want to install

- For the time being, or rather for this demo, we're not really installing anything, but in the future, in more 3D related demos, we'll be installing support for iOS and Android. But anywhoo, this should work just fine for the time being.

## 4 main parts

When you look at the unity window, there are 4 main parts to consider (I used to think these are random names, but its actually what they are called):

### Project Panel

This is what the project looks like:

![alt text](image.png)

For context, the project contains:

- Sprites (Like blended images in the simplest sense)
- Sound effects
- Scripts
- Tiles
- etc.

So what you can do, is take 2 images of a bird and a sprite and dump them in. I found some online and decided to use them.

### Hierarchy Panel

This is what this pane looks like:

![alt text](image-1.png)

Essentially, everything in this scene can be seen here. A scene (for the time being) could be understood to be something like a level.

So as you can see, for the time being, we have a camera and a light.

#### Creating a `GameObject`

Hit `Shift+Command+N` to create an empty (this is a GameObject).

An invisible container, with some position, rotation and scale to fill this container with components to add more value to this object. For example, if we add a `SpriteRenderer` component to this game object, we can add the Bird image on top of this.

Everything can be a game object, the bird, pipes and camera that is moving around as well.

The Panel that is responsible for the behaviour of each of these `GameObjects` is aptly called, the `Inspector`.

### Inspector Panel

This is the panel that is responsible for how the game object peforms in a situation. How does it react to gravity, how does it move around, etc.

Now in order to add the `sprite renderer`, all you have to do is:

Press `Add Component`

![alt text](image-2.png)

Type `Sprite Renderer`

![alt text](image-3.png)

Select the sprite that you want to render in this case:

![alt text](image-4.png)

### Scene Panel

This is the last panel, here you can see what is in the view at that point in time. In my case, its the following:

![Scene View](image-5.png)

### Big Bird

Now the bird is too big, how can we solve this.

You could just change the scale of the bird by messing with the game object, but you could also mess with the `MainCamera`

Click the `MainCamera` from the `Hierarchy Panel`.

You'd see the following in the inspector:

![alt text](image-6.png)

In order to change the size of the camera, head over to `Projection` and change it to to an arbitrary size.

While at it, head over to the `Environment` and change the colour of the background as well.

And this is it, this is the very first game, that does nothing.

