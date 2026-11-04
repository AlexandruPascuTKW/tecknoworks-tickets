# The video lens

A short narrated explainer in the 3Blue1Brown style: animated diagrams built up one sentence
at a time, a local neural voice, and burned-in subtitles. Build it only when the user asks.
It is a guided tour for a first pass, not a reference, so it complements the app rather than
replacing it.

## Contents
1. Toolchain (once per machine)
2. Script the narration from the model
3. Animate
4. Render, check, fix
5. Hand-off

## 1. Toolchain

```bash
bash scripts/video/setup_video_env.sh ~/.cache/knowledge-lenses/video-env
```

Run it in the background; the first run downloads about 400 MB. It is idempotent, and later
runs take seconds. It puts Manim and Kokoro TTS into one venv in that folder and installs
nothing system-wide. It needs Python 3.10–3.12, ffmpeg, and cairo + pango (macOS: Homebrew).
Problems it already handles on macOS, so don't rediscover them:

- pycairo needs `pkg-config`, so it installs the PyPI `pkgconf` into the venv.
- Homebrew cairo's `.pc` files need `zlib`/`bzip2`/`expat`/`libffi` `.pc` files that macOS
  doesn't ship, so it writes stubs pointing at the SDK.
- The espeak-ng bundled with kokoro-onnx on macOS ignores its data path and fails with
  `Error processing file '/Users/runner/work/espeakng-loader/.../phontab'`. Narration therefore
  goes text → `misaki` phonemes → Kokoro with `is_phonemes=True`, and espeak is never used.

**Voice.** Kokoro is free, local, and close to paid services for calm narration. The default
voice is `af_heart`; alternatives are `af_bella`, `am_michael` (US male) and `bm_george` (British
male). Don't use macOS `say`: users find it robotic. A cloud voice (ElevenLabs, OpenAI) sends the
narration text to a third party, so offer it only if the user asks, and only for material they
are fine sharing.

## 2. Script the narration from the model

Write the script from `model.json`, not from memory, so the video says the same thing as the
app. Aim for about 90–120 seconds: 5–7 beats with 2–5 sentences each. A good default arc:

1. What it is, in one sentence, and where it starts.
2. The main flow (from the map).
3. The part people get wrong (often a state machine or a distinction).
4. One concrete case (from a walkthrough, if there is one).
5. What is not settled (from questions).
6. A one-line close.

Write about 80% of the way to ASD-STE100: one claim per sentence, active voice, one term per
concept, no stacked clauses. Spoken audio can't be re-read, so this matters even more than in
text. Give a separate `spoken` string when the written form reads badly aloud, e.g. subtitle
"RFC 9111", spoken "R F C ninety-one eleven". The template warns about words the phonemizer doesn't know; respell
those in `spoken`.

## 3. Animate

```bash
cp scripts/video/explainer_template.py <topic_dir>/<topic>-explainer.py
```

Keep `NarratedScene`, `line()`, `box()` and `tts()`; replace the beats in `Explainer.construct`.
Each `self.line(subtitle, spoken, fn)` plays one sentence: it adds the audio, shows the subtitle,
runs `fn`'s animations, then waits out the rest of the audio. That keeps picture and voice in
sync without manual timing. Patterns that work:

- **Build up, don't reveal.** Add one element per sentence: box, arrow, label.
- **One idea per frame.** Call `clear_stage()` between beats.
- **Reuse the app's look.** Use status colours (green confirmed, amber inferred, red open) and the
  same node names as the map.
- **Use real names.** Show `ORD-1042` rather than "a record".
- **Mark open items visibly**, e.g. a red dashed arrow labelled "OPEN".
- **Keep the frame clear.** Stay within roughly y ∈ [−2.6, 3.4] so content never touches the
  subtitle band, and use `Text` only, since `MathTex` needs LaTeX installed.

## 4. Render, check, fix

Keep `media/` in a scratch directory, not in the topic folder.

```bash
E=~/.cache/knowledge-lenses/video-env
cd <topic_dir> && VIDEO_ENV=$E $E/.venv/bin/manim -ql --media_dir <scratch>/media --disable_caching <topic>-explainer.py Explainer
```

Then check the layout. You can't watch a video, so pull keyframes and look at them as a contact
sheet. Pick timestamps spread across the duration:

```bash
V=<scratch>/media/videos/<topic>-explainer/480p15/Explainer.mp4
ffprobe -v error -show_entries format=duration -of csv=p=0 "$V"
mkdir -p <scratch>/frames
for t in 10 25 40 55 70 85; do ffmpeg -v error -y -ss $t -i "$V" -frames:v 1 <scratch>/frames/f$t.png; done
cd <scratch>/frames && ffmpeg -v error -y -i f10.png -i f25.png -i f40.png -i f55.png -i f70.png -i f85.png \
  -filter_complex "[0][1]hstack[a];[2][3]hstack[b];[4][5]hstack[c];[a][b][c]vstack=inputs=3" contact.png
```

Read `contact.png`. Look for text overlapping lines or other text, content pushed off-centre,
labels clipped at the edges, and arrows running through labels. Fix and re-render. A frame
caught mid-animation can look faded; that's fine. When the layout is clean, render the final
version:

```bash
VIDEO_ENV=$E $E/.venv/bin/manim -r 1920,1080 --frame_rate 30 --media_dir <scratch>/media --disable_caching <topic>-explainer.py Explainer
```

## 5. Hand-off

Copy the final mp4 to `<topic_dir>/<topic>-explainer.mp4` and keep the `.py` next to it, so
the user can edit and re-render. Add `{"title": "Explainer video", "href": "<topic>-explainer.mp4"}`
to `model.media`, rebuild the app, and open the video for the user. Be honest that you can't
hear audio: say which voice you used and offer short samples of the alternatives.
