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

## Now onto the physics objects

### Adding `RigidBody`

So in order to add more physics related abilities to our bird, we can add the following:

![Rigid Body Addition](image-7.png)

Add `RigidBody2D` to the bird,this can be found under `Physics Objects`.

This would make your bird fall straight down, you can tweak things like the mass of the `GameObject` and how the effect of gravity on this object.

### Adding `CircleCollider` and some Mechanics

Now we want to see how this bird would interact with the pipes, so we would need to simulate some sort of interaction with the pipes. So adding a cirucle collider would allow us to understand how the objects would interact based on collisions.

In my case, this is what it looks like:

![Circle Collider](image-8.png)

So once you add the `CircleCollider`, its time to write some code which would dictate the way this interacts
with the rest of the pipes.

So add a new component, by selecting `New Script`.

Name the script whatever you want, I'm naming it `BirdScript`.

Once that is done, add the component, and time to edit some C# on Visual Studio Code on macos.

### Coding in C#

When you open the script, you woudld see the following code:

```csharp
using UnityEngine;

public class BirdScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
```

There are 2 methods that one would need to care more about:
- `Start()`
  - Start only **runs once at the start, and does only one action one time**.
- `Update()`
  - Update runs **once every frame, for as long as it needs to run**.

Since you're writing this for the `Bird` game object, you can use this to change the attributes of the bird real fast.

Not only the bird, but also the camera and other related phenomenon.

`start()` can be changed to something of this sort:

```csharp
void Start()
    {
        gameObject.name = "HEHE";
    }
```

I could change the name at the very start.

Or if you're like me, and want to cause as much chaos as possible, you could change the name of the `GameObject` to get longer and longer during the run.

In essence, this would cause it to never ever work (DONT DO IT)

#### Connecting the `RigidBody2D` to the `BirdScript`.

As of now, we have no interface between these 2. As a matter of fact, `BirdScript` can only access the 
top bit, and `Transform` bit, it cannot access the other components that we have just added to it. So how can this be done?

Well, you just need to add a reference to this particular component in the script.

This can be done as follows:

```csharp
public Rigidbody2D rigidbody2D;
```

Adding this, would certainly link this to the rigidbody2D component.

> In order to link this, you would still need to drag and drop this, when required.

In my case, I wanted this to fly, so I added a force constant in the y Direction, in this way:

```csharp
void Update()
    {
        rigidbody2D.AddForceY(1);
    }
```

This would make it fly away, but what if I wanted one tap, and then for it to fall down.

How could that happen?

Well, add it just to the start

```csharp
using UnityEngine;

public class BirdScript : MonoBehaviour
{

    // Add a reference to the RigidBody2 sector
    public Rigidbody2D rigidbody2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody2D.AddForceY(600);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
```

So this would make it jump a bit, and then fall down for ever.

But if you want to create something like a motion, you would need to use `linearVelocity` in C#.

This is what the code would look like:

```csharp
void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            rigidbody2D.linearVelocity = Vector2.up * 10;
        }
    }
```
> Whenever you detect a spacebar, add `Vector2.up` amount of velocity. For context Vector2.up is essentially (0,1) force in the Y direction.

