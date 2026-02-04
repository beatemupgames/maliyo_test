# Scene Fade System - Integration Instructions

## Overview
This fade system provides smooth fade in/out transitions between scenes without modifying existing code. It consists of two scripts:
- **SceneFadeManager**: Singleton that persists across scenes and handles fade effects
- **SceneFadeController**: Helper component for integrating fades with UnityEvents (buttons, etc.)

---

## Initial Setup

### 1. Create the Fade Manager GameObject (Menu Scene)

1. Open the **Menu.unity** scene
2. Create a new GameObject: `GameObject > Create Empty`
3. Name it: **"SceneFadeManager"**
4. Add the **SceneFadeManager** component to it

### 2. Create the Fade UI

1. With **SceneFadeManager** selected, add a Canvas:
   - `Right-click SceneFadeManager > UI > Canvas`
   - Name it: **"FadeCanvas"**
   - Set **Render Mode**: `Screen Space - Overlay`
   - Set **Sort Order**: `999` (to appear on top of everything)

2. Add a full-screen fade image:
   - `Right-click FadeCanvas > UI > Image`
   - Name it: **"FadeImage"**
   - Set **Color**: Black (RGB: 0, 0, 0, Alpha: 255)
   - Anchor it to fill the screen:
     - Select the **Anchor Presets** (top-left of Rect Transform)
     - Hold **Alt + Shift** and click **bottom-right preset** (stretch both)
     - Set all values (Left, Top, Right, Bottom) to `0`

### 3. Configure SceneFadeManager Component

Select the **SceneFadeManager** GameObject and configure:

**Fade Settings:**
- **Fade In Duration**: `0.5` (seconds) - adjust to taste
- **Fade Out Duration**: `0.5` (seconds) - adjust to taste
- **Fade Color**: Black `(0, 0, 0, 255)`
- **Fade Curve**: `AnimationCurve` (default is fine, or customize for different easing)
- **Auto Fade In On Scene Load**: ✅ Checked (enables automatic fade in)

**UI References:**
- **Fade Canvas**: Drag the **FadeCanvas** GameObject here
- **Fade Image**: Drag the **FadeImage** GameObject here

---

## Integration Without Code Modification

### Option A: Using SceneFadeController (Recommended)

This option requires NO CODE CHANGES to existing scripts.

#### For Menu Scene (Play Button)

1. Find the **Play Button** in the Menu scene hierarchy
2. Add a **SceneFadeController** component to the button GameObject:
   - Select the Play Button
   - `Add Component > SceneFadeController`

3. Configure SceneFadeController:
   - **Target Scene Name**: `"Game"` (the scene to load)

4. Modify the Button's OnClick event:
   - Remove or keep the existing `MenuManager.OnPlayButton()` call (it will still work)
   - Add a new event: `+ ` button
   - Drag the **Play Button** GameObject (with SceneFadeController) into the object slot
   - Select function: `SceneFadeController > TriggerFadeOutAndLoadScene()`

**Result**: When clicked, the button will fade out and then load the Game scene. The fade in will happen automatically.

#### For Game Scene (Back Button)

1. Open the **Game.unity** scene
2. Find the **Back Button** in the hierarchy
3. Add a **SceneFadeController** component to the button GameObject:
   - Select the Back Button
   - `Add Component > SceneFadeController`

4. Configure SceneFadeController:
   - **Target Scene Name**: `"Menu"` (the scene to return to)

5. Modify the Button's OnClick event:
   - Remove or keep the existing `BackButton.GoBack()` call
   - Add a new event: `+ ` button
   - Drag the **Back Button** GameObject (with SceneFadeController) into the object slot
   - Select function: `SceneFadeController > TriggerFadeOutAndLoadScene()`

**Result**: When clicked, the button will fade out and then return to the Menu scene.

---

### Option B: Direct Integration (Alternative)

If you prefer calling the fade directly from existing code:

#### Modify MenuManager.cs OnPlayButton():
```csharp
public void OnPlayButton()
{
    // Play button click sound
    PlayClickSound();

    // Save the selected difficulty to PlayerPrefs
    PlayerPrefs.SetString("GameDifficulty", currentDifficulty.ToString());
    PlayerPrefs.Save();

    // Fade out and load the game scene
    if (SceneFadeManager.Instance != null)
    {
        SceneFadeManager.Instance.FadeOutAndLoadScene(gameSceneName);
    }
    else
    {
        // Fallback if no fade manager
        SceneManager.LoadScene(gameSceneName);
    }
}
```

#### Modify BackButton.cs GoBack():
```csharp
public void GoBack()
{
    // Fade out and load the target scene
    if (SceneFadeManager.Instance != null)
    {
        SceneFadeManager.Instance.FadeOutAndLoadScene(targetSceneName);
    }
    else
    {
        // Fallback if no fade manager
        SceneManager.LoadScene(targetSceneName);
    }
}
```

---

## Testing

1. **Test Menu → Game transition:**
   - Play the Menu scene
   - The screen should fade in from black when the scene starts
   - Click the Play button
   - The screen should fade out to black, then load the Game scene
   - The Game scene should fade in from black

2. **Test Game → Menu transition:**
   - While in the Game scene, click the Back button
   - The screen should fade out to black, then return to Menu
   - The Menu should fade in from black

---

## Customization

### Adjusting Fade Timing
- Change **Fade In Duration** and **Fade Out Duration** in the SceneFadeManager Inspector
- Lower values = faster fades (e.g., `0.3`)
- Higher values = slower fades (e.g., `1.0`)

### Changing Fade Color
- Change **Fade Color** in the SceneFadeManager Inspector
- Default: Black (0, 0, 0)
- Try white for a different effect (255, 255, 255)

### Custom Easing
- Click the **Fade Curve** in the Inspector to open the Animation Curve editor
- Adjust the curve for different fade acceleration/deceleration
- Linear fade: Straight diagonal line
- Ease in/out: S-curve (default)
- Custom: Create your own curve

### Disable Auto Fade In
- Uncheck **Auto Fade In On Scene Load** if you want to manually control when fade in happens
- Call `SceneFadeManager.Instance.FadeIn()` from your own scripts when ready

---

## Advanced Usage

### Trigger Fades from Code
```csharp
// Fade in
SceneFadeManager.Instance.FadeIn();

// Fade out
SceneFadeManager.Instance.FadeOut();

// Fade out and load scene
SceneFadeManager.Instance.FadeOutAndLoadScene("SceneName");

// Check if currently fading
bool isFading = SceneFadeManager.Instance.IsFading();
```

### Use SceneFadeController Events
The **SceneFadeController** component has UnityEvents that fire when fades complete:
- **On Fade Out Complete**: Triggered when fade out finishes
- **On Fade In Complete**: Triggered when fade in finishes

Use these to chain actions or play sounds/effects when transitions complete.

---

## Troubleshooting

**Problem**: Fade doesn't appear
- Check that **FadeCanvas** is set to `Screen Space - Overlay`
- Check that **FadeImage** covers the entire screen (anchors stretched)
- Check that **Sort Order** is high enough (999+)

**Problem**: Fade happens but scene doesn't load
- Make sure **Target Scene Name** is spelled correctly
- Make sure the scene is added to **Build Settings** (File > Build Settings)

**Problem**: SceneFadeManager disappears between scenes
- The manager should persist via `DontDestroyOnLoad`
- Make sure you only have ONE SceneFadeManager in the first scene (Menu)
- Don't create multiple instances

**Problem**: Fades are too fast/slow
- Adjust **Fade In Duration** and **Fade Out Duration** values in Inspector

---

## Summary

✅ **No code modification required** - Use SceneFadeController with UnityEvents
✅ **All parameters serializable** - Configure everything in Inspector
✅ **Works in both Menu and Game scenes** - Automatic fade in on scene load
✅ **Persistent across scenes** - Single GameObject manages all transitions
✅ **Fallback support** - Gracefully handles missing fade manager

Enjoy your smooth scene transitions!
