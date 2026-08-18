# SPT-SWAY

---

# 🟣 JOIN THE DISCORD — https://discord.gg/nxa3W7w4rJ

### **https://discord.gg/nxa3W7w4rJ**

**This is the single most important link in this README.** All updates, release
announcements, bug fixes, early builds and support happen in the Discord **first**.
If you run this mod, join it — it is the only place you will reliably hear about
breaking changes and new versions.

**What's coming next:** I am building a **post-1.0 patcher and backend, written
from scratch and engineered to be very performant** — a proper foundation instead
of the current patchwork. **All of these mods will shortly be merged into that new
system.** If you want to follow that work, or use it when it lands, the Discord is
where it will be announced.

### 👉 **https://discord.gg/nxa3W7w4rJ** 👈

---


Weapon sway for SPT, built from the physical sources that actually move a rifle
instead of from a sine wave with a nice name.

Tarkov's stock sway is fine. It is also one thing: a wobble that scales with
ergonomics and stamina. Real sway is at least five separate things happening at
once on completely different timescales, and you can feel the difference
immediately — a slow breathing arc you can time a shot against, a sharp pulse
that ticks through a scope, a fine shimmer that never stops, a lazy balance
wander, and a heavy weapon lagging behind your turn. This mod models each of
those separately and lets you turn every one of them up, down, or off.

Around 150 config entries. That is not a boast, it is the point: if you can name
a part of the sway you dislike, there is a number for it.

---

## What it models

**Respiration.** Not a sine. A real breath rises quickly, falls more slowly, and
then sits still at the bottom for a moment before the next one starts. That
still moment is the whole reason breathing matters to a shooter — it is the
window you fire in — and a sine wave doesn't have one. Rate and depth follow
your actual stamina and oxygen. Holding your breath quiets it rather than
freezing it solid, and you pay for the hold with an overshoot when you let go.

**Cardiac.** Your heartbeat, transmitted up the support arm into the weapon.
Nearly invisible at rest and impossible to ignore through 4x after a sprint.
The waveform is a sharp systolic spike followed by the smaller dicrotic bump
where the aortic valve closes, which is the detail that stops a pulse reading
as "vertical sine wave" to the eye. Heart rate climbs faster than it falls,
because that's what hearts do, and firing raises it.

**Physiological tremor.** The 8–12 Hz shimmer every human hand has and no amount
of training removes. Layered detuned value noise, so it never repeats and never
parks at zero. Fatigue widens it, weapon weight widens it, a fractured arm
widens it a lot.

**Postural drift.** Sub-hertz wander from a standing body constantly correcting
its own balance. This is the one people forget, and it's why a rested shooter
with a light rifle still can't pin a dot to a target. It mostly disappears when
you go prone or rest the gun on something, which is exactly what it should do.

**Weapon inertia.** A real damped harmonic oscillator driven by your turn rate,
not a lerp back to zero. That matters because it means a heavy weapon doesn't
merely move *less* — it moves *later*, and settles for *longer*. Natural
frequency and damping ratio are both exposed, so you can tune the lag and the
overshoot independently. Weapon mass shifts the frequency on its own, which is
what makes an RSASS feel nothing like a PP-19.

On top of all that: stance and support (standing/crouched/prone/mounted/bipod,
blended rather than snapped), lean penalty, stamina and oxygen and arm stamina
as three separate drains, sustained-aim fatigue that builds while the weapon is
up, arm fractures and blacked limbs, and per-shot disturbance that stacks
through a burst and decays afterward.

Every source is centred over time, so nothing can quietly walk your point of aim
off target no matter how asymmetric its waveform is.

---

## Install

1. Grab `SptSway.dll` from [Releases](https://github.com/savannt/SPT-SWAY/releases).
2. Drop it in `BepInEx/plugins/SPT-SWAY/`.
3. Launch. `F10` toggles the mod, `F11` shows a live readout of everything the
   model is using.

Built against SPT 4.x. It patches `ProceduralWeaponAnimation`, reads
the player's physical state, and adds to BSG's own hand and camera springs
rather than replacing them.

---

## Presets

Pick one under `00 - General → Preset`, then tick **Apply Preset**. The tickbox
acts as a button and unticks itself. A preset overwrites the values it cares
about and leaves the rest alone, so treat it as a starting point rather than a
reset.

| Preset | What it's for |
|---|---|
| **Realistic** | The default. Numbers picked from how a body behaves, then tuned by feel. |
| **Vanilla** | Everything this mod adds, off. Stock EFT, for A/B comparison. |
| **Arcade** | Readable and forgiving. Sway you notice but never fight. |
| **Hardcore** | Realistic with the leash off. Fatigue and injury bite hard. |
| **Marksman** | Slow sources kept, fast ones trimmed. Long-range shooting. |
| **Cinematic** | Oversized and slow. Looks great on video, shoots badly. |

---

## Tuning it

The config is ordered the way the signal actually flows, top to bottom:

```
sources → ADS/hip weights → weapon handling → fatigue → injury
        → stance & support → shot disturbance → master → springs
```

A few things worth knowing before you start turning knobs:

- **`Master Intensity` first.** If everything is uniformly too much or too
  little, it's one number, and you're done.
- **Per-source amplitudes second.** Each source has its own `Amplitude`, its own
  axis weights (pitch/yaw/roll), and its own ADS and hipfire multipliers. Almost
  every complaint about sway is really a complaint about *one* of the five
  sources, so find that one.
- **`Camera Coupling` decides where the sway goes.** At 0 the gun moves and your
  view stays put. At 1 your head goes with it. This is the single most
  consequential setting in the mod: if your view wanders while your hand is
  still, or the whole thing feels nauseating, come here first and leave the
  amplitudes alone. The default is deliberately low (0.10), and each source has
  its own share underneath it — tremor's is near zero, because tremor on the
  camera reads as a buzz rather than as a hand.
- **`Output Smoothing` is the anti-jitter knob.** A low-pass across everything
  the model produces, just before it reaches the springs. Lower is smoother and
  eventually mushy; higher lets sharp detail like the pulse spike through. If
  the sway feels shaky rather than alive, drop it before you touch anything else.
- **`Drive Gain` is calibration, not taste.** It converts the model's output in
  degrees into the impulse BSG's springs want. Touch it only if the weapon feels
  like it's fighting the spring rather than riding it.
- **Section 13 is the deep end.** It scales BSG's own effectors — their breath,
  their tremor, their motion react, their walk bob — against the values the game
  shipped with. Off by default. It composes with everything above rather than
  replacing it, so turning both up gets you double.

`F11` while in a raid shows exertion, breath rate and phase, heart rate,
handling factor, support factor, and the actual degrees going to the hands and
the camera each frame. If a number isn't doing what you expect, that overlay
will tell you which stage ate it.

### About the defaults

The physiological numbers — breathing rates, tremor band, heart rate response,
the shape of the waveforms — are drawn from how those systems behave. The
*amplitudes* are a judgement call, because "how many degrees should a heartbeat
move a rifle in a video game" has no correct answer, only a good-feeling one.
They're a considered starting point and nothing more. Change them.

---

## Compatibility

It adds to the hand and camera springs the same way BSG's own effectors do, and
it captures the game's shipped effector values before scaling them, so it
composes with other animation mods rather than stamping on them.

Running it alongside another sway mod will stack, not conflict — you'll get both,
which is probably not what you want. Pick one.

---

## Building

```sh
dotnet build -c Release
```

Point it at your install with `-p:SptInstallDir="D:\SPT"` or the `SPT_INSTALL_DIR`
environment variable; it defaults to `D:\SPT`. A successful build copies the DLL
straight into `BepInEx/plugins/SPT-SWAY/`. Pass `-p:NoBepInExCopy=true` to skip
that.

References resolve out of the install directory, so you need SPT present to
build — there are no vendored game assemblies in this repo, and there won't be.

---

## License

MIT. See [LICENSE](LICENSE).
