# Blender Transforms in Unity 3D (BTU)

<!---%=description%-->

An editor script that allow the manipulation of GameObject’s position, rotation, and scale through the hotkeys that are used by Blender. Also allow quick hotkey resetting of the transform and hotkeys for manipulating the camera in scene view.

<!---%=obtain%-->

###Obtain!###
[Releases](https://github.com/cjddmut/Blender-Transforms-in-Unity3D/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/cjddmut/Blender-Transforms-in-Unity3D/archive/develop.zip) and copy the files in Assets to your project's Assets folder.

<!---%=docrest%-->

## Quick Action Hotkeys

### Reset Transform ###

**Alt+G - Reset Position**

This will set the local position of the object to (0, 0, 0).

**Alt+R - Reset Rotation**

This will set the local rotation of the object to (0, 0, 0).

**Alt+S - Reset Scale**

This will set the local scale of the object to (1, 1, 1).

## Transform Edit

You can now with the ease of hotkeys manipulate the transform of objects in the scene view. By selecting GameObject(s) in the scene window or hierarchy window, you can perform the following actions:

**G - Translate**

This will move the object parallel to the camera's view plane based off the motion of the mouse.

**R - Rotate**

This will rotate an object through an axis that is perpendicular to the camera's view plane. Considering the object's position as the center, the object will rotate based off of the angle of the mouse's original position and the current position of the mouse.

**S - Scale**

This will scale the object uniformly regardless of the viewing angle. Moving the mouse closer to the object's position will scale it down while moving away from the object will increase the scale.

Actions can be confirmed by pressing the hotkey that activated the transform again or by pressing enter. Actions can be canceled by pressing ESC or space.

### Single Axis Lock ###

While performing any transform action on an object. The action can be limited to a single axis by pressing the key for the axis desired to be locked. For example, to only rotate an object on the X axis then the key 'X' needs only be pressed while rotating an object. The same is true for the Y axis (key 'Y') and the Z axis (key 'Z').

The axis lock is in global space. To perform an axis lock in local space then press the desired axis key twice. For example, to translate an object forward (Z axis) then press 'Z' twice. The exception is for scaling an object. The scale action only performs in local space.

### Double Axis Lock ###

With any axis key press, if it is accompanied with shift then that axis is omitted from the axis lock allowing movement on the other two axis and restricting change on the selected axis. For example, if an object is already sitting on the ground and for it to remain on the flat ground by moved then by pressing Shift+Y then the object will move along its X and Z axis and there will be no change in the Y axis. Similar to single axis lock, the double axis lock first performs the lock in global space but by pressing the key combination again the lock will now be in local space.

### Increment Snapping ###

Any time while performing a transform edit, Ctrl may be pressed to toggle snapping. When snapping is enabled the object will snap to incremental values. For example, if enabled while translating then the object will snap to whole numbers when being moved, rotation snaps to 45 degree angles, and scale will snap to whole numbers.

### Configurable Options ###

In the BTU Configuration Window (Window -> Unity Made Awesome -> Blender Transforms in Unity) there are options for altering the behavior of the transform editting.

**Snap By Default** - If this is enabled then objects will snap by default when transform editing. When this is the case pressing Ctrl will toggle the non-snapping behavior.

**Translate Snap Increment** The increment from the global zero position to snap objects to. Default is 1.

**Rotate Snap Increment** The angle increment snap from zero rotation. Default is 45 degrees.

**Scale Snap Increment** The increment from no scaling (1, 1, 1) to snap the scaling to. Default is 1.

**'T' for Rotate** Use key 'T' instead of 'R' for rotate. This is different than Blender but possibly desirable as using 'R' will override the rotate widget change in the scene view.

**Enable Mouse (iffy)** Enable the mouse to be used to confirm or cancel transforms. If enabled then left click will confirm a transform edit while right click will cancel an edit. However, there are limitations and issues to using this feature. The click must occur in the scene view and after a right mouse click, an additional click with the left click is necessary in order to select and object.
<!---%title=Blender Transforms for Unity3D%-->
<!---%download=https://github.com/cjddmut/Blender-Transforms-in-Unity3D/releases/download/v0.1.0/BlenderTransformsInUnity-v0.1.0.unitypackage%-->
<!---%github=https://github.com/cjddmut/Blender-Transforms-in-Unity3D%-->
